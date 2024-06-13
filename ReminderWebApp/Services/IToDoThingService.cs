using ReminderWebApp.Data.Models;

namespace ReminderWebApp.Services
{
    public interface IToDoThingService
    {
        Task<List<ToDoThing>> GetTodayTodothings();
    }
}
