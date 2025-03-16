using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace vfstyle_backend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string content)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress(_configuration["EmailSettings:SmtpUsername"], "VF Style");
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = content;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient(_configuration["EmailSettings:SmtpServer"], 
                    int.Parse(_configuration["EmailSettings:SmtpPort"])))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(
                        _configuration["EmailSettings:SmtpUsername"], 
                        _configuration["EmailSettings:SmtpPassword"]);
                    
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