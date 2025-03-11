using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using SendGrid;
using vfstyle_backend.Models.Auth;
using System.Net.Mail;
using System.Net;

namespace vfstyle_backend.Services.Auth
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string content)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress(_emailSettings.SmtpUsername, _emailSettings.FromName ?? "Glasses Store");
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = content;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

                    await client.SendMailAsync(message);
                    return true;
                }
            }
            catch (Exception)
            {
                // Log exception
                return false;
            }
        }
    }
}
