using Microsoft.AspNetCore.Mvc;
using ReminderWebApp.Data;
using ReminderWebApp.Services;

namespace ReminderWebApp.ViewComponents
{
    public class ToDoThingsViewComponent : ViewComponent
    {
        private ApplicationDbContext _context;
        private IToDoThingService _toDoThingService;
        public ToDoThingsViewComponent(ApplicationDbContext context, IToDoThingService toDoThingService)
        {
            _context = context;
            _toDoThingService = toDoThingService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var todayTodothings = await _toDoThingService.GetTodayTodothings();
            return View(todayTodothings);
        }
    }
}
