using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Services.ToDoThingService;
using ReminderWebApp.Services.UserService;

namespace ReminderWebApp.Pages
{
    public class AllToDoThingsModel : PageModel
    {
        private IUserService _userService;
        private IToDoThingService _toDoThingService;
        public AllToDoThingsModel(IUserService userService, IToDoThingService toDoThingService)
        {
            _userService = userService;
            _toDoThingService = toDoThingService;
        }
        [BindProperty]
        public List<ToDoThing> ToDoThings { get; set; }
        public async Task<IActionResult> OnGet(DateTime? date)
        {
            var userId = await _userService.GetCurrentUserIdAsync();

            if(userId == null)
            {
                return Forbid();
            }

            ToDoThings = await _toDoThingService.GetUserTodayToDoThingsByDateAsync(userId, date);

            return Page();
        }
    }
}
