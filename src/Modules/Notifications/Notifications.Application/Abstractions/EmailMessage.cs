namespace Notifications.Application.Abstractions;

public record EmailMessage(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true);
