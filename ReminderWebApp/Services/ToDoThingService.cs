using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Models;

namespace ReminderWebApp.Services
{
    public class ToDoThingService : IToDoThingService
    {
        private ApplicationDbContext _context;

        public ToDoThingService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<ToDoThing>> GetTodayTodothings()
        {
            var today = DateTime.Today;
            var tomorrow = DateTime.Today.AddDays(1);

			return await _context.ToDoThings.Where(t => t.Date < tomorrow && t.Date > today).ToListAsync();
        }
    }
}
