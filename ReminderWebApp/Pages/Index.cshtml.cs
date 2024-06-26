using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace ReminderWebApp.Pages
{
	public class NewToDoThingModel
	{
		public required string Title { get; set; }
		public string? Description { get; set; }
		[DataType(DataType.Date)]
		public DateTime Date { get; set; }
		[DataType(DataType.Time)]
        public DateTime Time { get; set; }
	}

	public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private ApplicationDbContext _context;

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public NewToDoThingModel Input { get; set; }

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> OnGet()
        {
            var student = await _context.Students.ToListAsync();
            Name = student.FirstOrDefault().LastName;
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var newToDoThing = new ToDoThing
            {
                Date = new DateTime(Input.Date.Year, Input.Date.Month, Input.Date.Day, Input.Time.Hour, Input.Time.Minute, 0),
                Title = Input.Title,
                Description = Input.Description
            };
            _context.ToDoThings.Add(newToDoThing);
            await _context.SaveChangesAsync();
            return Page();
        }
    }
}
