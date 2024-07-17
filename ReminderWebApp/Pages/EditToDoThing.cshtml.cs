using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Models;
using ReminderWebApp.Services.ToDoThingService;
using ReminderWebApp.Services.UserService;
using SendGrid.Helpers.Errors.Model;
using System.ComponentModel.DataAnnotations;

namespace ReminderWebApp.Pages
{
    public class ToDoThingModelForEdit
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Необходимо ввести название события")]
        public required string Title { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Необходимо дату события")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "Необходимо ввести время события")]
        [DataType(DataType.Time)]
        public DateTime Time { get; set; }
        public TimeSpan RemindTime { get; set; }
    }

    [Authorize]
    public class EditToDoThingModel : PageModel
    {
        private IToDoThingService _toDoThingService;

        [BindProperty]
        public ToDoThingModelForEdit ToDoThing { get; set; }
        public EditToDoThingModel(IToDoThingService toDoThingService)
        {
            _toDoThingService = toDoThingService ?? throw new ArgumentNullException(nameof(toDoThingService));
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var toDoThing = await _toDoThingService.GetToDoThingByIdAsync(id);

                if (toDoThing == null)
                {
                    return Redirect("/Index");
                }

                var editModel = new ToDoThingModelForEdit
                {
                    Id = toDoThing.Id,
                    Title = toDoThing.Title,
                    Description = toDoThing.Description,
                    Date = toDoThing.Date,
                    RemindTime = TimeSpan.FromMinutes(toDoThing.RemindTime)
                };

                ToDoThing = editModel;

                return Page();
            }
            catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                await _toDoThingService.ChangeToDoThingAsync(id, ToDoThing.Title, ToDoThing.Description, ToDoThing.Date, ToDoThing.RemindTime.TotalMinutes);

                return Redirect("/ToDoThing/" + id);
            }
            catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
