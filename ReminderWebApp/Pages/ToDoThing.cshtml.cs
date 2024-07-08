using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReminderWebApp.Data.Models;
using ReminderWebApp.Data;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Services.ToDoThingService;
using ReminderWebApp.Services.UserService;
using Microsoft.AspNetCore.Authorization;

namespace ReminderWebApp.Pages
{
    [Authorize]
    public class ToDoThingModel : PageModel
    {
        private IToDoThingService _toDoThingService;
        private IUserService _userService;

        [BindProperty]
        public ToDoThing ToDoThing { get; set; }
        public ToDoThingModel(IToDoThingService toDoThingService, IUserService userService)
        {
            _toDoThingService = toDoThingService;
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var userId = await _userService.GetCurrentUserIdAsync();

                var isToDoThingUser = await _toDoThingService.IsCurrentUserToDoThing(id, userId);

                if (userId == null || !isToDoThingUser)
                {
                    return Forbid();
                }

                ToDoThing = await _toDoThingService.GetToDoThingByIdAsync(id);
                return Page();
            }
            catch
            {
                return Redirect("/Index");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var userId = await _userService.GetCurrentUserIdAsync();

                var isToDoThingUser = await _toDoThingService.IsCurrentUserToDoThing(id, userId);

                if (userId == null || !isToDoThingUser)
                {
                    return Forbid();
                }

                await _toDoThingService.DeleteToDoThingByIdAsync(userId, id);

                return Redirect("/AllToDoThings");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
