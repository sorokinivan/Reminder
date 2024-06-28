﻿using System.ComponentModel.DataAnnotations;

namespace ReminderWebApp.Data.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
