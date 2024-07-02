using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReminderWebApp.Data.Models;
using ReminderWebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ReminderWebApp.Pages
{
    public class ToDoThingModel : PageModel
    {
        private ApplicationDbContext _context;

        [BindProperty]
        public ToDoThing ToDoThing { get; set; }
        public ToDoThingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var result = await _context.ToDoThings.FirstOrDefaultAsync(t => t.Id == id);

            if (result == null)
            {
                return Redirect("Index");
            }

            ToDoThing = result;
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _context.ToDoThings.FirstOrDefaultAsync(t => t.Id == id);

            if(result == null)
            {
                return BadRequest();
            }

            _context.ToDoThings.Remove(result);

            await _context.SaveChangesAsync();

            return Redirect("/AllToDoThings");
        }
    }
}
