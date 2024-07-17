using System.Security.Claims;

namespace ReminderWebApp.Helpers.AuthorizationHelpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetCurrentUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
