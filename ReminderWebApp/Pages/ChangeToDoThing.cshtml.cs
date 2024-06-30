using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Models;

namespace ReminderWebApp.Pages
{
    public class ChangeToDoThingModel : PageModel
    {
        private ApplicationDbContext _context;

        [BindProperty]
        public ToDoThing ToDoThing { get; set; }
        public ChangeToDoThingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var result = await _context.ToDoThings.Where(t => t.Id == id).FirstOrDefaultAsync();

            if (result == null)
            {
                return Redirect("Index");
            }

            ToDoThing = result;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var toDoThing = await _context.ToDoThings.Where(t => t.Id == id).FirstOrDefaultAsync();
            if (toDoThing == null)
            {
                return BadRequest();
            }

            toDoThing.Title = ToDoThing.Title;
            toDoThing.Description = ToDoThing.Description;
            toDoThing.Date = ToDoThing.Date;

            await _context.SaveChangesAsync();

            return Page();
        }
    }
}
