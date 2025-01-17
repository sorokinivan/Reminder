﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Migrations;
using ReminderWebApp.Data.Models;
using SendGrid.Helpers.Errors.Model;
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

            return await _context.ToDoThings.Where(t => t.UserId == userId && t.Date < tomorrow && t.Date >= today && !t.IsDeleted).AsNoTracking().ToListAsync();
        }

        public async Task DeleteToDoThingByIdAsync(int id)
        {
            var result = await _context.ToDoThings.FirstOrDefaultAsync(t => t.Id == id);

            if (result == null)
            {
                Log.Logger.Error("{0} Не найдено событие с Id {1}", System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "Неизвестный метод", id);
                throw new NotFoundException("Такого события не существует");
            }

            result.IsDeleted = true;

            await _context.SaveChangesAsync();

            Log.Logger.Information("Удалено событие с Id {0}", result.Id);
        }
        public async Task<ToDoThing> GetToDoThingByIdAsync(int id)
        {
            var result = await _context.ToDoThings.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (result == null)
            {
                Log.Logger.Error("{0} Не найдено событие с Id {1}", System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "Неизвестный метод", id);
                throw new NotFoundException("Такого события не существует");
            }

            return result;
        }
        public async Task<List<ToDoThing>> GetUserToDoThingsByDateAsync(string? userId, DateTime? date)
        {
            if(userId == null)
            {
                throw new NotFoundException("Пользователь не найден");
            }
            var query = _context.ToDoThings.Where(t => t.UserId == userId && !t.IsDeleted);

            if (date != null)
            {
                var nextDay = date.Value.AddDays(1);
                var toDt = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day);

                query = query.Where(t => t.UserId == userId && t.Date < toDt && t.Date >= date.Value.Date);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task ChangeToDoThingAsync(int id, string title, string description, DateTime date, double remindTime)
        {
            var toDoThing = await GetToDoThingByIdAsync(id);

            if(toDoThing == null)
            {
                Log.Logger.Error("{0} Не найдено событие с Id {1}",System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "Неизвестный метод", id);
                throw new NotFoundException("Такого события не существует");
            }

            toDoThing.Title = title;
            toDoThing.Description = description;
            toDoThing.Date = date;
            toDoThing.RemindTime = remindTime;

            await _context.SaveChangesAsync();

            Log.Logger.Information("Изменено событие с Id {0}", toDoThing.Id);
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
            var dateEndCurrentMonth = new DateTime(now.Year, now.Month + 1, 1, 0, 0, 0);

            return await _context.ToDoThings.Where(t => t.UserId == userId && t.Date >= dateStartCurrentMonth && t.Date < dateEndCurrentMonth).AsNoTracking().Select(t => t.Date.Day).Distinct().ToListAsync();
        }

        public async Task<bool> IsCurrentUserToDoThingAsync(int id, string? userId)
        {
            if(userId == null)
            {
                return false;
            }

            var toDoThingUserId = await _context.ToDoThings.Where(t => t.Id == id).AsNoTracking().Select(t => t.UserId).FirstOrDefaultAsync();

            if(toDoThingUserId == null)
            {
                throw new NotFoundException("Такого события не существует");
            }

            return userId == toDoThingUserId;
        }
    }
}
