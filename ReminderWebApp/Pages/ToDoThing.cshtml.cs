using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Data;

namespace ReminderWebApp.Pages
{
    public class ToDoThingModel : PageModel
    {
        private ApplicationDbContext _context;

        
        public class ToDoThing
        {
            public required string Title { get; set; }
            public string? Description { get; set; }
        }
        [BindProperty]
        public ToDoThing toDoThing { get; set; }
        public ToDoThingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var result = await _context.ToDoThings.Where(t => t.Id == id).Select(t => new ToDoThing{ Title = t.Title, Description = t.Description }).FirstOrDefaultAsync();

            if(result == null)
            {
                return Redirect("Index");
            }

            toDoThing = result;
            return Page();
        }
    }
}
