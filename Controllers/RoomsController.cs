using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using room_for_lease_api.Helpers;
using room_for_lease_api.Models;

namespace room_for_lease_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public RoomsController(AppDbContext db)
        {
            _db = db;
        }

        // Tenant: Xem phòng đang thuê
        [HttpGet("my-room")]
        [Authorize(Roles = "Tenant")]
        public async Task<ActionResult<Room>> GetMyRoom()
        {
            var userEmail = this.GetCurrentUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized();

            var contract = await _db.Contracts
                .Include(c => c.Tenant)
                .Include(c => c.Room)
                    .ThenInclude(r => r.Owner)
                .Where(c => c.Tenant.Email == userEmail && c.Status == ContractStatus.Active)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefaultAsync();

            if (contract == null)
                return NotFound("No active contract found");

            return contract.Room;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<Room>>> GetRooms([FromQuery] RoomQuery query)
        {
            var rooms = _db.Rooms.AsNoTracking().Include(r => r.Owner).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var keyword = query.Keyword.Trim();
                rooms = rooms.Where(r => r.Title.Contains(keyword) || r.Address.Contains(keyword));
            }
            if (query.IsAvailable.HasValue)
            {
                rooms = rooms.Where(r => r.IsAvailable == query.IsAvailable.Value);
            }
            if (query.MinPrice.HasValue)
            {
                rooms = rooms.Where(r => r.Price >= query.MinPrice.Value);
            }
            if (query.MaxPrice.HasValue)
            {
                rooms = rooms.Where(r => r.Price <= query.MaxPrice.Value);
            }

            var totalItems = await rooms.CountAsync();
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 10 : Math.Min(query.PageSize, 100);

            var items = await rooms
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Room>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Room>> GetById(int id)
        {
            var room = await _db.Rooms.Include(r => r.Owner).FirstOrDefaultAsync(r => r.Id == id);
            if (room == null) return NotFound();

            // Nếu user là Tenant, kiểm tra xem họ có đang thuê phòng này không
            if (User.Identity?.IsAuthenticated == true)
            {
                var userRole = this.GetCurrentUserRole();
                var userEmail = this.GetCurrentUserEmail();

                if (userRole == "Tenant" && !string.IsNullOrEmpty(userEmail))
                {
                    var hasActiveContract = await _db.Contracts
                        .Include(c => c.Tenant)
                        .AnyAsync(c => c.RoomId == id && 
                                      c.Tenant.Email == userEmail && 
                                      c.Status == ContractStatus.Active);
                    
                    if (!hasActiveContract)
                        return Forbid("You don't have access to this room");
                }
            }

            return room;
        }

        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<ActionResult<Room>> Create(CreateRoomRequest request)
        {
            if (request == null)
                return BadRequest("Room data is required");

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Title is required");

            if (string.IsNullOrWhiteSpace(request.Address))
                return BadRequest("Address is required");

            if (request.Price <= 0)
                return BadRequest("Price must be greater than 0");

            if (request.Area <= 0)
                return BadRequest("Area must be greater than 0");

            // Verify OwnerId exists
            var ownerExists = await _db.Users.AnyAsync(u => u.Id == request.OwnerId);
            if (!ownerExists)
                return BadRequest("OwnerId does not exist");

            var room = new Room
            {
                Title = request.Title,
                Price = request.Price,
                Area = request.Area,
                IsAvailable = request.IsAvailable,
                Address = request.Address,
                Description = request.Description,
                OwnerId = request.OwnerId, // Set FK only, NOT navigation property
                CreatedAt = DateTime.UtcNow
            };

            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Update(int id, Room update)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            room.Title = update.Title;
            room.Description = update.Description;
            room.Address = update.Address;
            room.Price = update.Price;
            room.Area = update.Area;
            room.IsAvailable = update.IsAvailable;
            room.OwnerId = update.OwnerId;
            room.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Delete(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Room-Tenant endpoints
        [HttpGet("{id}/tenants")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<ActionResult<IEnumerable<RoomTenantResponse>>> GetRoomTenants(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            var contracts = await _db.Contracts
                .Where(c => c.RoomId == id && c.Status == ContractStatus.Active)
                .Include(c => c.Tenant)
                .ToListAsync();

            var result = contracts.Select(c => new RoomTenantResponse
            {
                Id = c.Tenant.Id,
                FullName = c.Tenant.FullName,
                Phone = c.Tenant.Phone,
                Email = c.Tenant.Email,
                Address = c.Tenant.Address,
                ContractId = c.Id,
                StartDate = c.StartDate,
                MonthlyRent = c.MonthlyRent
            }).ToList();

            return result;
        }

        [HttpPost("{id}/tenants")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<ActionResult> AddTenantToRoom(int id, AddTenantToRoomRequest request)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            var tenant = await _db.Tenants.FindAsync(request.TenantId);
            if (tenant == null) return NotFound("Tenant not found");

            // Check if tenant already has active contract in this room
            var existingContract = await _db.Contracts
                .FirstOrDefaultAsync(c => c.RoomId == id && c.TenantId == request.TenantId && c.Status == ContractStatus.Active);
            
            if (existingContract != null)
                return BadRequest("Tenant already has an active contract in this room");

            var contract = new Contract
            {
                TenantId = request.TenantId,
                RoomId = id,
                StartDate = request.StartDate ?? DateTime.UtcNow.Date,
                MonthlyRent = request.MonthlyRent ?? room.Price,
                Status = ContractStatus.Active
            };

            _db.Contracts.Add(contract);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRoomTenants), new { id }, tenant);
        }

        [HttpDelete("{id}/tenants/{tenantId}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<ActionResult> RemoveTenantFromRoom(int id, int tenantId)
        {
            var contract = await _db.Contracts
                .FirstOrDefaultAsync(c => c.RoomId == id && c.TenantId == tenantId && c.Status == ContractStatus.Active);
            
            if (contract == null) return NotFound();

            contract.Status = ContractStatus.Terminated;
            contract.TerminatedAt = DateTime.UtcNow;
            contract.EndDate = DateTime.UtcNow.Date;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Invoice endpoints
        [HttpGet("{id}/invoices")]
        [Authorize(Roles = "Owner,Admin,Tenant")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetRoomInvoices(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            var userRole = this.GetCurrentUserRole();
            var userEmail = this.GetCurrentUserEmail();

            var query = _db.Invoices.Where(i => i.RoomId == id);

            // Tenant chỉ xem được hóa đơn của mình
            if (userRole == "Tenant" && !string.IsNullOrEmpty(userEmail))
            {
                var contractIds = await _db.Contracts
                    .Include(c => c.Tenant)
                    .Where(c => c.RoomId == id && 
                               c.Tenant.Email == userEmail && 
                               c.Status == ContractStatus.Active)
                    .Select(c => c.Id)
                    .ToListAsync();

                if (!contractIds.Any())
                    return Forbid("You don't have access to this room's invoices");

                query = query.Where(i => contractIds.Contains(i.ContractId));
            }

            var invoices = await query
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return invoices;
        }
    }

    public class AddTenantToRoomRequest
    {
        public int TenantId { get; set; }
        public DateTime? StartDate { get; set; }
        public decimal? MonthlyRent { get; set; }
    }
}



