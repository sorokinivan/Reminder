using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ReminderWebApp.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailLogin = _config["emailSenderEmail"];
            var password = _config["emailSenderPassword"];

            SmtpClient client = new SmtpClient
            {
                Port = 587,
                Host = "smtp-mail.outlook.com",
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailLogin, password)
            };

            return client.SendMailAsync(emailLogin, email, subject, htmlMessage);
        }
    }
}
