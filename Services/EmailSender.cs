using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
namespace COMP2139_Assignment1.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var emailSettings = _config.GetSection("EmailSettings");

        var host = emailSettings["Host"] ?? throw new InvalidOperationException("SMTP Host is missing.");
        var port = int.TryParse(emailSettings["Port"], out var parsedPort) ? parsedPort : 587;
        var username = emailSettings["Username"] ?? throw new InvalidOperationException("SMTP Username is missing.");
        var password = emailSettings["Password"] ?? throw new InvalidOperationException("SMTP Password is missing.");
        var fromEmail = emailSettings["FromEmail"] ?? throw new InvalidOperationException("FromEmail is missing.");

        var smtpClient = new SmtpClient
        {
            Host = host,
            Port = port,
            EnableSsl = true,
            Credentials = new NetworkCredential(username, password)
        };

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        message.To.Add(email);

        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogError("SMTP host not configured.");
            return;
        }
        await smtpClient.SendMailAsync(message);
    }
}