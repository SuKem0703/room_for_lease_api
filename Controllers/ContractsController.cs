using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using room_for_lease_api.Helpers;
using room_for_lease_api.Models;

namespace room_for_lease_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ContractsController(AppDbContext db)
        {
            _db = db;
        }

        // Tenant: Xem hợp đồng của mình
        [HttpGet("my-contract")]
        [Authorize(Roles = "Tenant")]
        public async Task<ActionResult<Contract>> GetMyContract()
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

            return contract;
        }

        public class CreateContractRequest
        {
            public int TenantId { get; set; }
            public int RoomId { get; set; }
            public DateTime StartDate { get; set; }
            public decimal MonthlyRent { get; set; }
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<Contract>> Create(CreateContractRequest req)
        {
            var tenant = await _db.Tenants.FindAsync(req.TenantId);
            var room = await _db.Rooms.FindAsync(req.RoomId);
            if (tenant == null || room == null)
            {
                return NotFound();
            }

            var contract = new Contract
            {
                TenantId = req.TenantId,
                RoomId = req.RoomId,
                StartDate = req.StartDate == default ? DateTime.UtcNow.Date : req.StartDate.Date,
                MonthlyRent = req.MonthlyRent,
                Status = ContractStatus.Active
            };
            _db.Contracts.Add(contract);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Owner,Admin,Tenant")]
        public async Task<ActionResult<Contract>> GetById(int id)
        {
            var contract = await _db.Contracts
                .Include(c => c.Tenant)
                .Include(c => c.Room)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (contract == null) return NotFound();

            var userRole = this.GetCurrentUserRole();
            var userEmail = this.GetCurrentUserEmail();

            // Tenant chỉ xem được hợp đồng của mình
            if (userRole == "Tenant" && contract.Tenant.Email != userEmail)
                return Forbid();

            return contract;
        }

        [HttpPut("{id}/terminate")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult> Terminate(int id)
        {
            var contract = await _db.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            if (contract.Status == ContractStatus.Terminated) return NoContent();
            contract.Status = ContractStatus.Terminated;
            contract.TerminatedAt = DateTime.UtcNow;
            contract.EndDate = DateTime.UtcNow.Date;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}



