using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ReminderWebApp.Services.UserService
{
    public class UserService : IUserService
    {
        private IHttpContextAccessor _httpContextAccessor;
        private UserManager<IdentityUser> _userManager;
        public UserService(IHttpContextAccessor httpContextAccessor ,UserManager<IdentityUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<string?> GetCurrentUserIdAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if(user == null)
            {
                return null;
            }

            var result = await _userManager.GetUserAsync(user);

            return result == null ? null : result.Id;
        }
    }
}
