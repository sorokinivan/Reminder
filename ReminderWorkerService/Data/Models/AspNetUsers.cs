using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderWorkerService.Data.Models
{
    public class AspNetUsers
    {
        public required string Id { get; set; }
        public string? Email { get; set; }
        public long? TelegramChatId { get; set; }
    }
}
