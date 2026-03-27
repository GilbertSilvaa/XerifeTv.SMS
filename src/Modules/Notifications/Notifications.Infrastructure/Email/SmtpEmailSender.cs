using Microsoft.Extensions.Options;
using Notifications.Application.Abstractions;
using System.Net;
using System.Net.Mail;

namespace Notifications.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly SmtpClient _smtpClient;

    public SmtpEmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
        _smtpClient = new SmtpClient(_emailSettings.Host)
        {
            Port = _emailSettings.Port,
            Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password),
            EnableSsl = _emailSettings.EnableSsl
        };
    }

    public async Task SendAsync(EmailMessage email, CancellationToken cancellationToken = default)
    {
        var mailMessage = new MailMessage()
        {
            From = new(_emailSettings.From),
            Subject = email.Subject,
            Body = await TemplateRenderer.RenderAsync(email.Template, email.TemplateKeyValues),
            IsBodyHtml = true
        };

        mailMessage.To.Add(email.To);
        await _smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }
}