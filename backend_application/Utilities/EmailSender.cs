using System.Net;
using System.Net.Mail;

namespace backend_application.Utilities;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
        {
            Port = int.Parse(_configuration["Smtp:Port"]),
            Credentials = new NetworkCredential(
                _configuration["Smtp:Username"], 
                _configuration["Smtp:Password"]
            ),
            EnableSsl = bool.Parse(_configuration["Smtp:EnableSsl"])
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["Smtp:From"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(to);

        await smtpClient.SendMailAsync(mailMessage);
    }
}