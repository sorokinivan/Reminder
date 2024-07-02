using System.Net.Mail;
using System.Net;

namespace EmailSenderLibrary
{
    public class EmailSender
    {
        public async Task SendEmailAsync(string emailLogin, string emailPassword, string fromEmail, string[] recipients, string subject, string body)
        {
            using (var client = new SmtpClient("smtp-mail.outlook.com", 587)
            {
                Credentials = new NetworkCredential("sorokinivan670@gmail.com", "14082021Spb21082021"),
                EnableSsl = true
            })
            {
                var from = new MailAddress(fromEmail);

                var message = new MailMessage()
                {
                    Subject = subject,
                    Body = body
                };

                message.From = from;

                foreach (var recipient in recipients)
                {
                    message.To.Add(recipient);
                }

                await client.SendMailAsync(message);
            }
        }
    }
}
