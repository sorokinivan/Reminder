using ReminderWebApp.Data.Models;

namespace ReminderWebApp.Services.ToDoThingService
{
    public interface IToDoThingService
    {
        Task<List<ToDoThing>> GetUserTodayToDoThings(string userId);
        Task DeleteToDoThingByIdAsync(int id);
        Task<ToDoThing> GetToDoThingByIdAsync(int id);
        Task<List<ToDoThing>> GetUserToDoThingsByDateAsync(string? userId, DateTime? date);
        Task ChangeToDoThingAsync(int id, string title, string description, DateTime date, double remindTime);
        Task AddNewToDoThingAsync(string userId, string title, string description, DateTime date, DateTime time, double remindTime);
        Task<List<int>> GetUserToDoThingsDaysForCurrentMonthAsync(string? userId);
    }
}
