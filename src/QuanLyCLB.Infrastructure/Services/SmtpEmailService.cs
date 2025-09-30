using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using QuanLyCLB.Application.Interfaces;
using QuanLyCLB.Infrastructure.Settings;

namespace QuanLyCLB.Infrastructure.Services;

public class SmtpEmailService(IOptions<SmtpSettings> options) : IEmailService
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            throw new InvalidOperationException("SMTP settings are not configured.");
        }

        using var message = new MailMessage();
        message.From = new MailAddress(
            string.IsNullOrWhiteSpace(_settings.FromEmail)
                ? _settings.Username
                : _settings.FromEmail,
            string.IsNullOrWhiteSpace(_settings.FromName)
                ? _settings.FromEmail
                : _settings.FromName);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = isHtml;

        message.To.Add(new MailAddress(to));

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        await using var registration = cancellationToken.Register(client.SendAsyncCancel);
        await client.SendMailAsync(message, cancellationToken);
    }
}
