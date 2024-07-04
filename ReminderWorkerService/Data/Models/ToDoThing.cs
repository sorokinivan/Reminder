using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ReminderWorkerService.Data.Models
{
    public class ToDoThing
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
        public required string UserId { get; set; }
        public double RemindTime { get; set; }
        [ForeignKey("UserId")]
        public AspNetUsers? AspNetUser { get; set; }
    }
}
