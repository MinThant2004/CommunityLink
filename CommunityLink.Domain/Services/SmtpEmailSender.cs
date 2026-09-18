using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using Microsoft.Extensions.Logging;

namespace CommunityLink.Domain.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly CustomSettingModel _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(CustomSettingModel settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default)
    {
        var smtp = _settings.SmtpSettings;

        if (string.IsNullOrWhiteSpace(smtp.Host) || string.IsNullOrWhiteSpace(smtp.Username))
        {
            _logger.LogWarning("SMTP Host or Username is not configured. Email to {ToEmail} skipped.", toEmail);
            return;
        }

        try
        {
            using var client = new SmtpClient(smtp.Host, smtp.Port)
            {
                Credentials = new NetworkCredential(smtp.Username, smtp.Password),
                EnableSsl = smtp.EnableSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(smtp.FromEmail, smtp.FromName),
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email successfully sent to {ToEmail} with subject '{Subject}'", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail} with subject '{Subject}'", toEmail, subject);
            throw;
        }
    }
}
