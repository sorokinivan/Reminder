using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Models;
using ReminderWebApp.Services.ToDoThingService;
using ReminderWebApp.Services.UserService;
using Serilog;
using System.ComponentModel.DataAnnotations;

namespace ReminderWebApp.Pages
{
	public class NewToDoThingModel
	{
        [Required(ErrorMessage = "Необходимо ввести название события")]
        public required string Title { get; set; }
		public string? Description { get; set; }
        [Required(ErrorMessage = "Необходимо ввести дату события")]
        [DataType(DataType.Date)]
		public DateTime Date { get; set; }
        [Required(ErrorMessage = "Необходимо ввести время события")]
        [DataType(DataType.Time)]
        public DateTime Time { get; set; }
        public TimeSpan RemindTime { get; set; }
	}

	public class IndexModel : PageModel
    {
        private IUserService _userService;
        private IToDoThingService _toDoThingService;

        [BindProperty]
        public List<int> DaysWithToDoThings { get; set; }

        [BindProperty]
        public NewToDoThingModel Input { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IUserService userService, IToDoThingService toDoThingService)
        {
            _userService = userService;
            _toDoThingService = toDoThingService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = await _userService.GetCurrentUserIdAsync();

            DaysWithToDoThings = await _toDoThingService.GetUserToDoThingsDaysForCurrentMonthAsync(userId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var userId = await _userService.GetCurrentUserIdAsync();

                if (userId == null)
                {
                    return Forbid();
                }

                await _toDoThingService.AddNewToDoThingAsync(userId, Input.Title, Input.Description, Input.Date, Input.Time, Input.RemindTime.TotalMinutes);

                if (DaysWithToDoThings != null && !DaysWithToDoThings.Contains(Input.Date.Day))
                {
                    DaysWithToDoThings.Add(Input.Date.Day);
                }

                return Page();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
