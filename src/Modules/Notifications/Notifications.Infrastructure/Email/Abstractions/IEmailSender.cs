namespace Notifications.Infrastructure.Email.Abstractions;

public interface IEmailSender
{
    Task SendAsync(EmailMessage email, CancellationToken cancellationToken = default!);
}