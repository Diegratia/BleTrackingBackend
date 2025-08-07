using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper.Execution;
using Microsoft.Extensions.Configuration;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string toEmail, string username, string confirmationCode);
    Task SendVisitorInvitationEmailAsync(string toEmail, string name, string invitationCode, string invitationUrl, string memberName, DateTime? visitorPeriodStart, DateTime? visitorPeriodEnd);
    Task SendVisitorNotificationEmailAsync();
    Task SendMemberNotificationEmailAsync();
    // Task SendVisitorInvitationEmailAsync(string toEmail, string name, string confirmationCode);

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

    public async Task SendVisitorInvitationEmailAsync(string toEmail, string name, string invitationCode, string invitationUrl, string memberName, DateTime? visitPeriodStart, DateTime? visitPeriodEnd)
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
            Subject = "Visitor Invitation",
            Body = $@"
    Dear {name},

    You have been invited as a visitor.

    Visit Period: {visitPeriodStart} - {visitPeriodEnd}

    Please confirm your invitation and complete your data by clicking the link below:

    {invitationUrl}

    Invitation Code: {invitationCode}

    This link will expire in 3 days.

    Thank you,
    {memberName}",
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

    public async Task SendVisitorNotificationEmailAsync()
    {
        throw new NotImplementedException();
    }
    public async Task SendMemberNotificationEmailAsync()
    {
        throw new NotImplementedException();
    }


    // public async Task SendVisitorInvitationEmailAsync(string toEmail, string name, string confirmationCode)
    // {
    //     var smtpHost = _configuration["Email:SmtpHost"];
    //     var smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
    //     var smtpUsername = _configuration["Email:SmtpUsername"];
    //     var smtpPassword = _configuration["Email:SmtpPassword"];
    //     var fromEmail = _configuration["Email:FromEmail"];
    //     var fromName = _configuration["Email:FromName"];

    //     var message = new MailMessage
    //     {
    //         From = new MailAddress(fromEmail, fromName),
    //         Subject = "Visitor Invitation",
    //         Body = $@"Dear {name},

    //         You have been invited as a visitor. Please confirm your invitation by clicking the link below:

    //         Confirmation Code: {confirmationCode}

    //         This link will expire in 7 days.

    //         Thank you,
    //         Your Application Team",
    //         IsBodyHtml = false
    //     };
    //     message.To.Add(toEmail);

    //     using var client = new SmtpClient(smtpHost, smtpPort)
    //     {
    //         Credentials = new NetworkCredential(smtpUsername, smtpPassword),
    //         EnableSsl = true
    //     };

    //     await client.SendMailAsync(message);
    // }
}














// using System;
// using System.Net;
// using System.Net.Mail;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Configuration;

// namespace BusinessLogic.Services.Implementation
// {
//     public interface IEmailService
//     {
//         Task SendConfirmationEmailAsync(string toEmail, string username, string confirmationCode);
//     }

//     public class EmailService : IEmailService
//     {
//         private readonly IConfiguration _configuration;

//         public EmailService(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }

//         public async Task SendConfirmationEmailAsync(string toEmail, string username, string confirmationCode)
//         {
//             var smtpHost = _configuration["Email:SmtpHost"];
//             var smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
//             var smtpUsername = _configuration["Email:SmtpUsername"];
//             var smtpPassword = _configuration["Email:SmtpPassword"];
//             var fromEmail = _configuration["Email:FromEmail"];
//             var fromName = _configuration["Email:FromName"];

//             var message = new MailMessage
//             {
//                 From = new MailAddress(fromEmail, fromName),
//                 Subject = "Email Confirmation",
//                 Body = $@"Dear {username},

//                 Please confirm your email by entering the following code in the confirmation page:

//                 Confirmation Code: {confirmationCode}

//                 This code will expire in 7 days.

//                 Thank you,
//                 Your Application Team",
//                 IsBodyHtml = false
//             };
//             message.To.Add(toEmail);

//             using var client = new SmtpClient(smtpHost, smtpPort)
//             {
//                 Credentials = new NetworkCredential(smtpUsername, smtpPassword),
//                 EnableSsl = true
//             };

//             await client.SendMailAsync(message);
//         }
//     }
// }























