using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace room_for_lease_api.Helpers
{
    public static class ControllerExtensions
    {
        public static int? GetCurrentUserId(this ControllerBase controller)
        {
            var userIdClaim = controller.User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                ?? controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return null;
            
            return userId;
        }

        public static string? GetCurrentUserEmail(this ControllerBase controller)
        {
            return controller.User.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? controller.User.FindFirstValue(ClaimTypes.Email);
        }

        public static string? GetCurrentUserRole(this ControllerBase controller)
        {
            return controller.User.FindFirstValue(ClaimTypes.Role);
        }
    }
}

