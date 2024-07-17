using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReminderWebApp.Data.Models;
using ReminderWebApp.Data;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Services.ToDoThingService;
using ReminderWebApp.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using SendGrid.Helpers.Errors.Model;

namespace ReminderWebApp.Pages
{
    [Authorize]
    public class ToDoThingModel : PageModel
    {
        private IToDoThingService _toDoThingService;
        private readonly IAuthorizationService _authorizationService;

        [BindProperty]
        public ToDoThing ToDoThing { get; set; }
        public ToDoThingModel(IToDoThingService toDoThingService, IAuthorizationService authorizationService)
        {
            _toDoThingService = toDoThingService ?? throw new ArgumentNullException(nameof(toDoThingService));

            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                ToDoThing = await _toDoThingService.GetToDoThingByIdAsync(id);

                var authResult = await _authorizationService.AuthorizeAsync(User, ToDoThing, "IsToDoThingOwner");
                if (authResult.Succeeded)
                {
                    return Page();
                }
                else
                {
                    return Forbid();
                }
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _toDoThingService.DeleteToDoThingByIdAsync(id);

                return Redirect("/AllToDoThings");
            }
            catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
