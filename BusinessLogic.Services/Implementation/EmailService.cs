using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic.Services.Implementation
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string username, string confirmationCode);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string username, string confirmationCode)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
            var smtpUsername = _configuration["Email:SmtpUsername"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"];

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = "Email Confirmation",
                Body = $@"Dear {username},

                Please confirm your email by entering the following code in the confirmation page:

                Confirmation Code: {confirmationCode}

                This code will expire in 7 days.

                Thank you,
                Your Application Team",
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}