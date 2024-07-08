using Microsoft.AspNetCore.Identity;

namespace ReminderWebApp.Services.UserService
{
    public interface IUserService
    {
        Task<string?> GetCurrentUserIdAsync();
    }
}
