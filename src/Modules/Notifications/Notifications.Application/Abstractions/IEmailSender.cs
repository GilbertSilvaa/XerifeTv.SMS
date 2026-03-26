namespace Notifications.Application.Abstractions;

public interface IEmailSender
{
    Task SendAsync(EmailMessage email, CancellationToken cancellationToken = default!);
}