using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Migrations;
using ReminderWebApp.Data.Models;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReminderWebApp.Services.ToDoThingService
{
    public class ToDoThingService : IToDoThingService
    {
        private ApplicationDbContext _context;

        public ToDoThingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ToDoThing>> GetUserTodayToDoThings(string userId)
        {
            var today = DateTime.Today;
            var tomorrow = DateTime.Today.AddDays(1);

            return await _context.ToDoThings.Where(t => t.UserId == userId && t.Date < tomorrow && t.Date > today && !t.IsDeleted).ToListAsync();
        }

        public async Task DeleteToDoThingByIdAsync(string userId, int id)
        {
            var result = await _context.ToDoThings.FirstOrDefaultAsync(t => t.Id == id);

            if (result == null)
            {
                Log.Logger.Error("{0} - Не найдено событие с Id {1}", System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "Неизвестный метод", id);
                throw new Exception("Такого события не существует");
            }

            result.IsDeleted = true;

            await _context.SaveChangesAsync();

            Log.Logger.Information("Удалено событие с Id {0} пользователем {1}", result.Id, userId);
        }
        public async Task<ToDoThing?> GetToDoThingByIdAsync(int id)
        {
            var result = await _context.ToDoThings.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (result == null)
            {
                Log.Logger.Error("{0} Не найдено событие с Id {1}", System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "Неизвестный метод", id);
                throw new Exception("Такого события не существует");
            }

            return result;
        }
        public async Task<List<ToDoThing>> GetUserTodayToDoThingsByDateAsync(string userId, DateTime? date)
        {
            var query = _context.ToDoThings.Where(t => t.UserId == userId && !t.IsDeleted);

            if (date != null)
            {
                var ToDt = date.Value.AddDays(1);

                query = query.Where(t => t.Date < ToDt && t.Date > date.Value.Date);
            }

            return await query.ToListAsync();
        }

        public async Task ChangeToDoThingAsync(string userId, int id, string title, string description, DateTime date, double remindTime)
        {
            var toDoThing = await GetToDoThingByIdAsync(id);

            if(toDoThing == null)
            {
                Log.Logger.Error("{0} Не найдено событие с Id {1}",System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "Неизвестный метод", id);
                throw new Exception("Такого события не существует");
            }

            toDoThing.Title = title;
            toDoThing.Description = description;
            toDoThing.Date = date;
            toDoThing.RemindTime = remindTime;

            await _context.SaveChangesAsync();

            Log.Logger.Information("Изменено событие с Id {0} пользователем {1}", toDoThing.Id, userId);
        }

        public async Task AddNewToDoThingAsync(string userId, string title, string description, DateTime date, DateTime time, double remindTime)
        {
            var newToDoThing = new ToDoThing
            {
                Date = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0),
                Title = title,
                Description = description,
                UserId = userId,
                RemindTime = remindTime
            };
            _context.ToDoThings.Add(newToDoThing);
            
            await _context.SaveChangesAsync();

            Log.Information("Создано новое событие с Id {0} пользователем {1}", newToDoThing.Id, userId);
        }

        public async Task<List<int>> GetUserToDoThingsDaysForCurrentMonthAsync(string? userId)
        {
            if(userId == null)
            {
                return new List<int>();
            }

            var now = DateTime.Now;
            var dateStartCurrentMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
            var dateEndCurrentMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 0, 0, 0);

            var result = await _context.ToDoThings.Where(t => t.UserId == userId && t.Date > dateStartCurrentMonth && t.Date < dateEndCurrentMonth).Select(t => t.Date.Day).Distinct().ToListAsync();

            return result;
        }

        public async Task<bool> IsCurrentUserToDoThing(int id, string userId)
        {
            if(userId == null)
            {
                return false;
            }

            var toDoThingUserId = await _context.ToDoThings.Where(t => t.Id == id).Select(t => t.UserId).FirstOrDefaultAsync();

            return userId == toDoThingUserId;
        }
    }
}
