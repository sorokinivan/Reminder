using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReminderWebApp.Data;
using ReminderWebApp.Services.ToDoThingService;
using ReminderWebApp.Services.UserService;

namespace ReminderWebApp.ViewComponents
{
    public class ToDoThingsViewComponent : ViewComponent
    {
        private IToDoThingService _toDoThingService;
        private IHttpContextAccessor _httpContextAccessor;
        private UserManager<IdentityUser> _userManager;
        public ToDoThingsViewComponent(IToDoThingService toDoThingService, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _toDoThingService = toDoThingService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;

            if (claimsPrincipal == null)
            {
                return View();
            }

            var user = await _userManager.GetUserAsync(claimsPrincipal);

            if (user == null)
            {
                return View();
            }

            var todayTodothings = await _toDoThingService.GetUserTodayToDoThings(user.Id);
            return View(todayTodothings);
        }
    }
}
