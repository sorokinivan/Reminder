using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderWorkerService.Data.Models
{
    public class ToDoThing
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
        public string UserId { get; set; }
        public double RemindTime { get; set; }
    }
}
