using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper.Execution;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string toEmail, string username, string confirmationCode);
    Task SendVisitorInvitationEmailAsync(
    string toEmail,
    string name,
    string invitationCode,
    string invitationUrl,
    string? visitorPeriodStart,
    string? visitorPeriodEnd,
    string? visitorPeriodStartTime,
    string? visitorPeriodEndTime,
    string? invitationAgenda,
    string? maskedAreaName,
    string? memberName,
    string? floorName,
    string? buildingName
    );
    Task SendMemberInvitationEmailAsync(
    string toEmail,
    string name,
    string invitationCode,
    string memberInvitationUrl,
    string? invitationAgendaMember,
    string? startMemberDate,
    string? endMemberDate,
    string? startMemberTime,
    string? endMemberTime,
    string? maskedAreaMemberName,
    string? PurposePersonName,
    string? floorNameMember,
    string? buildingNameMember
    );

    Task SendMemberNotificationEmailAsync(
        string toEmail,         // email member (host)
        string hostName,        // nama member (host)
        string visitorName,     // nama visitor yang diundang
        string? invitationAgenda,
        string dateText,
        string timeText,
        string location,
        string memberName,
        string? confirmationCode,
        string invitationUrl);
    
                
    // Task SendVisitorInvitationEmailAsync(string toEmail, string name, string confirmationCode);

}

public class EmailService : IEmailService
{

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public EmailService(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    private async Task<string> LoadEmailTemplateAsync(string fileName)
    {
        // docker 
        var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplate", fileName);

        if (!File.Exists(templatePath))
        {
            // local
            templatePath = Path.Combine(_env.ContentRootPath, "..", "..", "Helpers.Consumer", "EmailTemplate", fileName);
        }

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Email template {fileName} not found at {templatePath}");


        templatePath = Path.GetFullPath(templatePath);

        

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Email template {fileName} not found.", templatePath);

        return await File.ReadAllTextAsync(templatePath);
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

    // public async Task SendVisitorInvitationEmailAsync(string toEmail, string name, string invitationCode, string invitationUrl, string visitorPeriodStart, string visitorPeriodEnd, string invitationAgenda, string maskedAreaName, string memberName, string floorName, string buildingName)
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
    //         Body = $@"
    // Dear {name},

    // You have been invited as a visitor.

    // Visit Period : {visitorPeriodStart} - {visitorPeriodEnd}

    // Agenda : {invitationAgenda}

    // Location : {floorName} - {maskedAreaName} - {buildingName}

    // Please confirm your invitation and complete your data by clicking the link below:

    // {invitationUrl}

    // Invitation Code: {invitationCode}

    // This link will expire in 3 days.

    // Thank you,
    // {memberName}",
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
    
   public async Task SendVisitorInvitationEmailAsync(
    string toEmail,
    string name,
    string invitationCode,
    string invitationUrl,
    string visitorPeriodStartDate,
    string visitorPeriodEndDate,
    string visitorPeriodStartTime,
    string visitorPeriodEndTime,
    string invitationAgenda,
    string maskedAreaName,
    string memberName,
    string floorName,
    string buildingName)
{
    var smtpHost = _configuration["Email:SmtpHost"];
    var smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
    var smtpUsername = _configuration["Email:SmtpUsername"];
    var smtpPassword = _configuration["Email:SmtpPassword"];
    var fromEmail = _configuration["Email:FromEmail"];
    var fromName = _configuration["Email:FromName"];

    var template = await LoadEmailTemplateAsync("SendVisitorInvitationEmailAsync.html");

    // Replace placeholder sesuai parameter
        var bodyHtml = template
        .Replace("%to_mail%", name)
        .Replace("%agenda%", invitationAgenda)
        .Replace("%date%", visitorPeriodStartDate + " - " + visitorPeriodEndDate)
        .Replace("%time%", visitorPeriodStartTime + " - " + visitorPeriodEndTime) 
        .Replace("%location%", $"{floorName} - {maskedAreaName} - {buildingName}")
        .Replace("%link%", invitationUrl)
        .Replace("%host%", memberName)
        .Replace("%code%", invitationCode)
        .Replace("%member%", memberName);

    var message = new MailMessage
    {
        From = new MailAddress(fromEmail, fromName),
        Subject = $"Visitor Invitation - {invitationAgenda}",
        Body = bodyHtml,
        IsBodyHtml = true
    };

    message.To.Add(toEmail);

    using var client = new SmtpClient(smtpHost, smtpPort)
    {
        Credentials = new NetworkCredential(smtpUsername, smtpPassword),
        EnableSsl = true
    };

    await client.SendMailAsync(message);
}




    public async Task SendMemberInvitationEmailAsync(
    string toEmail,
    string name,
    string invitationCode,
    string memberInvitationUrl,
    string? invitationAgendaMember,
    string? startMemberDate,
    string? endMemberDate,
    string? startMemberTime,
    string? endMemberTime,
    string? maskedAreaMemberName,
    string? PurposePersonName,
    string? floorNameMember,
    string? buildingNameMember
   )

    
{
    var smtpHost = _configuration["Email:SmtpHost"];
    var smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
    var smtpUsername = _configuration["Email:SmtpUsername"];
    var smtpPassword = _configuration["Email:SmtpPassword"];
    var fromEmail = _configuration["Email:FromEmail"];
    var fromName = _configuration["Email:FromName"];

    var template = await LoadEmailTemplateAsync("SendMemberInvitationEmailAsync.html");

    var bodyHtml = template
        .Replace("%to_mail%", name)
        .Replace("%agenda%", invitationAgendaMember)
        .Replace("%date%", startMemberDate + " - " + endMemberDate)
        .Replace("%time%", startMemberTime + " - " + endMemberTime) 
        .Replace("%location%", $"{floorNameMember} - { maskedAreaMemberName} - { buildingNameMember} ")
        .Replace("%link%", memberInvitationUrl)
        .Replace("%host%", PurposePersonName)
        .Replace("%code%", invitationCode)
        .Replace("%member%", PurposePersonName);

    var message = new MailMessage
    {
        From = new MailAddress(fromEmail, fromName),
        Subject = $"Meeting Invitation - {invitationAgendaMember}",
        Body = bodyHtml,
        IsBodyHtml = true
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
    public async Task SendMemberNotificationEmailAsync(
        string toEmail,         // email member (host)
        string hostName,        // nama member (host)
        string visitorName,     // nama visitor yang diundang
        string? invitationAgenda,
        string dateText,
        string timeText,
        string location,
        string memberName,
        string? confirmationCode,
        string? invitationUrl
    )
    {

        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = _configuration.GetValue<int>("Email:SmtpPort");
        var smtpUsername = _configuration["Email:SmtpUsername"];
        var smtpPassword = _configuration["Email:SmtpPassword"];
        var fromEmail = _configuration["Email:FromEmail"];
        var fromName = _configuration["Email:FromName"];

        var template = await LoadEmailTemplateAsync("MemberNotification.html");

        var bodyHtml = template
       .Replace("%to_mail%", hostName)
       .Replace("%agenda%", invitationAgenda)
       .Replace("%date%", dateText)
       .Replace("%time%", timeText)
       .Replace("%location%", $"{location}")
    //    .Replace("%link%", memberInvitationUrl)
       .Replace("%host%", memberName)
       .Replace("%code%", confirmationCode)
       .Replace("%member%", memberName)
       .Replace("%title%", "Your invitation has been sent")
       .Replace("%message%", $"Your invitation for <b>{visitorName}</b> has been created successfully.")
       .Replace("%cta_text%", invitationUrl);



       var message = new MailMessage
    {
        From = new MailAddress(fromEmail, fromName),
        Subject = $"Meeting Invitation - Your invitation for {visitorName} has been created successfully",
        Body = bodyHtml,
        IsBodyHtml = true
    };

    message.To.Add(toEmail);

    using var client = new SmtpClient(smtpHost, smtpPort)
    {
        Credentials = new NetworkCredential(smtpUsername, smtpPassword),
        EnableSsl = true
    };

    await client.SendMailAsync(message);
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























