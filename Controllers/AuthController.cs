using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using room_for_lease_api.Models;
using room_for_lease_api.Services;

namespace room_for_lease_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IJwtTokenService _jwt;

        public AuthController(AppDbContext db, IJwtTokenService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            var token = _jwt.GenerateToken(user);
            return new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString()
            };
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required");

            if (string.IsNullOrWhiteSpace(request.FullName))
                return BadRequest("Full name is required");

            if (request.Role == UserRole.Tenant && string.IsNullOrWhiteSpace(request.Phone))
                return BadRequest("Phone is required for Tenant registration");

            if (request.Password.Length < 6)
                return BadRequest("Password must be at least 6 characters");

            // Check if email already exists
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
                return BadRequest("Email already exists");

            // Create new user
            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                Role = request.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Auto-create Tenant if role is Tenant
            int? tenantId = null;
            if (request.Role == UserRole.Tenant)
            {
                var tenant = new Tenant
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = request.Phone
                };
                _db.Tenants.Add(tenant);
                await _db.SaveChangesAsync();
                tenantId = tenant.Id;
            }

            // Generate token and return
            var token = _jwt.GenerateToken(user);
            return new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                TenantId = tenantId
            };
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<AuthResponse>> Me()
        {
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            // Get TenantId if user is Tenant
            int? tenantId = null;
            if (user.Role == UserRole.Tenant)
            {
                var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.UserId == userId);
                tenantId = tenant?.Id;
            }

            return new AuthResponse
            {
                Token = string.Empty,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                TenantId = tenantId
            };
        }
    }
}



