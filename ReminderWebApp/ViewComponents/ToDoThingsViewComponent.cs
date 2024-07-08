using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReminderWebApp.Data;
using ReminderWebApp.Services.ToDoThingService;
using ReminderWebApp.Services.UserService;

namespace ReminderWebApp.ViewComponents
{
    public class ToDoThingsViewComponent : ViewComponent
    {
        private ApplicationDbContext _context;
        private IToDoThingService _toDoThingService;
        private IUserService _userService;
        public ToDoThingsViewComponent(ApplicationDbContext context, IToDoThingService toDoThingService, IUserService userService)
        {
            _context = context;
            _toDoThingService = toDoThingService;
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = await _userService.GetCurrentUserIdAsync();

            if (userId == null)
            {
                return View();
            }

            var todayTodothings = await _toDoThingService.GetUserTodayToDoThings(userId);
            return View(todayTodothings);
        }
    }
}
