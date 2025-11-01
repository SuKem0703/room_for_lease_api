using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using room_for_lease_api.Helpers;
using room_for_lease_api.Models;

namespace room_for_lease_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public InvoicesController(AppDbContext db)
        {
            _db = db;
        }

        // Tenant: Xem tất cả hóa đơn của mình
        [HttpGet("my-invoices")]
        [Authorize(Roles = "Tenant")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetMyInvoices()
        {
            var userEmail = this.GetCurrentUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized();

            // Tìm contracts của tenant qua email
            var contracts = await _db.Contracts
                .Include(c => c.Tenant)
                .Where(c => c.Tenant.Email == userEmail && c.Status == ContractStatus.Active)
                .Select(c => c.Id)
                .ToListAsync();

            if (!contracts.Any())
                return new List<Invoice>();

            var invoices = await _db.Invoices
                .Where(i => contracts.Contains(i.ContractId))
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return invoices;
        }

        // Tenant: Xem chi tiết hóa đơn
        [HttpGet("{id}")]
        [Authorize(Roles = "Tenant,Owner,Admin")]
        public async Task<ActionResult<Invoice>> GetById(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Room)
                .Include(i => i.Contract)
                    .ThenInclude(c => c.Tenant)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            var userRole = this.GetCurrentUserRole();
            var userEmail = this.GetCurrentUserEmail();

            // Tenant chỉ xem được hóa đơn của mình
            if (userRole == "Tenant")
            {
                if (invoice.Contract.Tenant.Email != userEmail)
                    return Forbid();
            }

            return invoice;
        }

        // Tenant: Thanh toán hóa đơn
        [HttpPut("{id}/pay")]
        [Authorize(Roles = "Tenant")]
        public async Task<ActionResult> PayInvoice(int id, PayInvoiceRequest request)
        {
            var userEmail = this.GetCurrentUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized();

            var invoice = await _db.Invoices
                .Include(i => i.Contract)
                    .ThenInclude(c => c.Tenant)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            // Kiểm tra tenant có quyền thanh toán hóa đơn này
            if (invoice.Contract.Tenant.Email != userEmail)
                return Forbid();

            if (invoice.Status == InvoiceStatus.Paid)
                return BadRequest("Invoice already paid");

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAmount = request.PaidAmount ?? invoice.Amount;
            invoice.PaidDate = DateTime.UtcNow;
            invoice.Notes = request.Notes;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Owner/Admin: Tạo hóa đơn
        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<ActionResult<Invoice>> Create(CreateInvoiceRequest request)
        {
            var contract = await _db.Contracts
                .Include(c => c.Room)
                .FirstOrDefaultAsync(c => c.Id == request.ContractId);

            if (contract == null)
                return NotFound("Contract not found");

            if (contract.Status != ContractStatus.Active)
                return BadRequest("Contract is not active");

            // Generate invoice number
            var invoiceNumber = $"INV-{DateTime.UtcNow:yyyy-MM}-{contract.Id}-{DateTime.UtcNow:HHmmss}";

            // Calculate total amount if not provided
            var totalAmount = request.Amount > 0 
                ? request.Amount 
                : request.ElectricCost + request.WaterCost + request.ServiceCost + request.RoomRent;

            var invoice = new Invoice
            {
                RoomId = contract.RoomId,
                ContractId = contract.Id,
                InvoiceNumber = invoiceNumber,
                IssueDate = request.IssueDate ?? DateTime.UtcNow.Date,
                DueDate = request.DueDate ?? DateTime.UtcNow.Date.AddDays(7),
                Period = request.Period,
                
                // Electricity
                ElectricOldReading = request.ElectricOldReading,
                ElectricNewReading = request.ElectricNewReading,
                ElectricConsumption = request.ElectricConsumption,
                ElectricUnitPrice = request.ElectricUnitPrice,
                ElectricCost = request.ElectricCost,
                
                // Water
                WaterOldReading = request.WaterOldReading,
                WaterNewReading = request.WaterNewReading,
                WaterConsumption = request.WaterConsumption,
                WaterUnitPrice = request.WaterUnitPrice,
                WaterCost = request.WaterCost,
                
                // Service
                ServiceCost = request.ServiceCost,
                ServiceUnitPrice = request.ServiceUnitPrice,
                
                // Room Rent
                RoomRent = request.RoomRent,
                RoomRentUnitPrice = request.RoomRentUnitPrice,
                
                // Deposit
                Deposit = request.Deposit,
                
                // Total
                Amount = totalAmount,
                
                Status = InvoiceStatus.Pending,
                Notes = request.Notes
            };

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
        }

        // Owner/Admin: Xem tất cả hóa đơn
        [HttpGet]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetAll([FromQuery] int? roomId)
        {
            var query = _db.Invoices.AsQueryable();

            if (roomId.HasValue)
            {
                query = query.Where(i => i.RoomId == roomId.Value);
            }

            var invoices = await query
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return invoices;
        }
    }

    public class PayInvoiceRequest
    {
        public decimal? PaidAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateInvoiceRequest
    {
        public int ContractId { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Period { get; set; } = string.Empty;
        
        // Electricity
        public decimal? ElectricOldReading { get; set; }
        public decimal? ElectricNewReading { get; set; }
        public decimal? ElectricConsumption { get; set; }
        public decimal ElectricUnitPrice { get; set; }
        public decimal ElectricCost { get; set; }
        
        // Water
        public decimal? WaterOldReading { get; set; }
        public decimal? WaterNewReading { get; set; }
        public decimal? WaterConsumption { get; set; }
        public decimal WaterUnitPrice { get; set; }
        public decimal WaterCost { get; set; }
        
        // Service (Rác + Wifi)
        public decimal ServiceCost { get; set; }
        public decimal ServiceUnitPrice { get; set; }
        
        // Room Rent
        public decimal RoomRent { get; set; }
        public decimal RoomRentUnitPrice { get; set; }
        
        // Deposit (optional)
        public decimal? Deposit { get; set; }
        
        // Total amount (calculated or manual)
        public decimal Amount { get; set; }
        
        public string? Notes { get; set; }
    }
}

