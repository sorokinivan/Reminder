using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace ReminderWebApp.Pages
{
    public class ToDoThingModelForEdit
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [DataType(DataType.Time)]
        public DateTime Time { get; set; }
        public TimeSpan RemindTime { get; set; }
    }
    public class EditToDoThingModel : PageModel
    {
        private ApplicationDbContext _context;

        [BindProperty]
        public ToDoThingModelForEdit ToDoThing { get; set; }
        public EditToDoThingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var result = await _context.ToDoThings.Where(t => t.Id == id).Select(t => new ToDoThingModelForEdit
            {
                Title = t.Title,
                Description = t.Description,
                Date = t.Date,
                RemindTime = TimeSpan.FromMinutes(t.RemindTime)
            }).FirstOrDefaultAsync();

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
            toDoThing.RemindTime = ToDoThing.RemindTime.TotalMinutes;

            await _context.SaveChangesAsync();

            return Redirect("/ToDoThing/"+id);
        }
    }
}
