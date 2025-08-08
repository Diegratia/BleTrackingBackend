using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper.Execution;
using Microsoft.Extensions.Configuration;

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
    string invitationUrl,
    string? visitorPeriodStart,
    string? visitorPeriodEnd
    // string? memberName
    );
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
    string visitorPeriodStart,
    string visitorPeriodEnd,
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

    var template = @"
<html lang=""en"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
    <meta name=""viewport"" content=""width=320, initial-scale=1"" />
    <title>Visitor Invitation - %agenda%</title>
</head>
<body style=""font-family: arial"">
    <table style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td width=""20%"">&nbsp;</td>
            <td width=""60%"">
                <div style=""background-color: #f4f4f4; padding: 0px 20px;"">
                    <table width=""100%"" style=""border-collapse: collapse"">
                        <tr>
                            <td colspan=""4"" style=""padding: 6px"">
                                <div>Halo %kepada%,</div>
                                <div style=""margin-bottom: 5px"">Anda diundang untuk menghadiri kunjungan dengan detail berikut:</div>
                            </td>
                        </tr>
                        <tr>
                            <td style=""padding: 6px"">Tanggal</td>
                            <td style=""text-align: center"">:</td>
                            <td colspan=""2"" style=""padding: 6px"">%tanggal%</td>
                        </tr>
                        <tr>
                            <td style=""padding: 6px"">Waktu</td>
                            <td style=""text-align: center"">:</td>
                            <td colspan=""2"" style=""padding: 6px"">%waktu%</td>
                        </tr>
                        <tr>
                            <td style=""padding: 6px"">Lokasi</td>
                            <td style=""text-align: center"">:</td>
                            <td colspan=""2"" style=""padding: 6px"">%lokasi%</td>
                        </tr>
                        <tr>
                            <td style=""padding: 6px"">Host/Organizaer</td>
                            <td style=""text-align: center"">:</td>
                            <td colspan=""2"" style=""padding: 6px"">%host%</td>
                        </tr>
                        <tr>
                            <td style=""padding: 6px"">Agenda</td>
                            <td style=""text-align: center"">:</td>
                            <td colspan=""2"" style=""padding: 6px"">%agenda%</td>
                        </tr>
                        <tr>
                            <td colspan=""4"" style=""padding-top: 10px;"">
                                <div>Silakan klik link berikut untuk mengonfirmasi dan melengkapi data:</div>
                                <div><a href=""%link%"" target=""_blank"">%link%</a></div>
                            </td>
                        </tr>
                        <tr>
                            <td colspan=""4"" style=""padding-top: 10px;"">
                                <div>Kode Undangan:</div>
                                <div style=""background-color: #9a9a9a; color: #fff; padding: 10px; width: max-content; font-size: 20px"">%kode%</div>
                            </td>
                        </tr>
                        <tr>
                            <td colspan=""4"" style=""padding-top: 10px;"">
                                <div>Hormat kami,</div>
                                <div>%member%</div>
                            </td>
                        </tr>
                        <tr>
                            <td colspan=""4"" style=""padding: 20px 0px;"">&nbsp;</td>
                        </tr>
                        <tr>
                            <td colspan=""4"" style=""background-color: #9a9a9a; text-align: center; padding: 20px;"">
                                Support By <a href=""https://bio-experience.com"" target=""_blank"">Bio Experience</a>
                            </td>
                        </tr>
                    </table>
                </div>
            </td>
            <td width=""20%"">&nbsp;</td>
        </tr>
    </table>
</body>
</html>";

    // Replace placeholder sesuai parameter
    var bodyHtml = template
        .Replace("%kepada%", name)
        .Replace("%agenda%", invitationAgenda)
        .Replace("%tanggal%", visitorPeriodStart + " - " + visitorPeriodEnd)
        .Replace("%waktu%", visitorPeriodStart + " - " + visitorPeriodEnd) // Bisa dipisah jam kalau perlu
        .Replace("%lokasi%", $"{floorName} - {maskedAreaName} - {buildingName}")
        .Replace("%link%", invitationUrl)
        .Replace("%host%", memberName)
        .Replace("%kode%", invitationCode)
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
string invitationUrl,
string visitorPeriodStart,
string visitorPeriodEnd)
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
            Subject = "Meeting Invitation",
            Body = $@"
        Dear {name},

        You have been invited to attend a meeting.

        Scheduled Time : {visitorPeriodStart} - {visitorPeriodEnd}
        Invited By     : 

        Please confirm your invitation and check the meeting details by clicking the link below:

        {invitationUrl}

        Invitation Code: {invitationCode}

        This link will expire in 3 days.

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























