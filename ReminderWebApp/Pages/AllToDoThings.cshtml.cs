using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ReminderWebApp.Pages
{
    public class AllToDoThingsModel : PageModel
    {
        private ApplicationDbContext _context;
        private UserManager<IdentityUser> _userManager;
        public AllToDoThingsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [BindProperty]
        public List<ToDoThing> ToDoThings { get; set; }
        public async Task<IActionResult> OnGet(DateTime? date)
        {
            var user = await _userManager.GetUserAsync(User);

            if(user == null)
            {
                return Forbid();
            }

            var query = _context.ToDoThings.Where(t => t.UserId == user.Id);

            if(date != null)
            {
                var ToDt = date.Value.AddDays(1);

                query = query.Where(t => t.Date < ToDt && t.Date > date.Value.Date);
            }

            ToDoThings = await query.ToListAsync();

            return Page();
        }
    }
}
