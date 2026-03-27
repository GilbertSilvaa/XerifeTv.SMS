namespace Notifications.Application.Abstractions;

public record EmailMessage(
    string To,
    string Subject,
    string Template, 
    Dictionary<string, string> TemplateKeyValues);
