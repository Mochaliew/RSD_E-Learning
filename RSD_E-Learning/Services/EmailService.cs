using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

public class EmailService : IEmailService
{
    private readonly DB _db;

    public EmailService(DB db)
    {
        _db = db;
    }

    public async Task SendAsync(string toEmail, string subject, string body)
    {
        var settings = await _db.SystemSettings.FirstAsync();

        if (!settings.EnableEmailNotification)
            return;

        var smtp = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
        {
            Credentials = new NetworkCredential(
                settings.SenderEmail,
                settings.SmtpPassword
            ),
            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress(settings.SenderEmail!, settings.PlatformName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(toEmail);

        await smtp.SendMailAsync(mail);
    }
}
