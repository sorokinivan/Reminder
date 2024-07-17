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
using Microsoft.AspNetCore.Authorization;
using ReminderWebApp.Helpers.AuthorizationHelpers;
using Microsoft.AspNetCore.Http;
using SendGrid.Helpers.Errors.Model;

namespace ReminderWebApp.Pages
{
    [Authorize]
    public class AllToDoThingsModel : PageModel
    {
        private IToDoThingService _toDoThingService;
        public AllToDoThingsModel(IToDoThingService toDoThingService)
        {
            _toDoThingService = toDoThingService ?? throw new ArgumentNullException(nameof(toDoThingService));
        }
        [BindProperty]
        public List<ToDoThing> ToDoThings { get; set; }
        public async Task<IActionResult> OnGet(DateTime? date)
        {
            try
            {
                ToDoThings = await _toDoThingService.GetUserToDoThingsByDateAsync(User.GetCurrentUserId(), date);

                return Page();
            }
            catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
