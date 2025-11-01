using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using room_for_lease_api.Models;

namespace room_for_lease_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TenantsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<Tenant>>> GetAll()
        {
            var list = await _db.Tenants.AsNoTracking().ToListAsync();
            return list;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<Tenant>> GetById(int id)
        {
            var tenant = await _db.Tenants.Include(t => t.Contracts).FirstOrDefaultAsync(t => t.Id == id);
            if (tenant == null) return NotFound();
            return tenant;
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<Tenant>> Create(Tenant tenant)
        {
            tenant.Id = 0;
            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var tenant = await _db.Tenants
                .Include(t => t.Contracts)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (tenant == null) return NotFound();

            // Terminate all active contracts before deleting tenant
            var activeContracts = tenant.Contracts.Where(c => c.Status == ContractStatus.Active).ToList();
            foreach (var contract in activeContracts)
            {
                contract.Status = ContractStatus.Terminated;
                contract.TerminatedAt = DateTime.UtcNow;
                contract.EndDate = DateTime.UtcNow.Date;
            }

            // Delete tenant (contracts will be deleted via cascade, but room remains)
            _db.Tenants.Remove(tenant);
            await _db.SaveChangesAsync();
            
            return NoContent();
        }
    }
}



