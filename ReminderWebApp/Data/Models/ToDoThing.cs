namespace ReminderWebApp.Data.Models
{
    public class ToDoThing
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
    }
}
