namespace Notifications.Infrastructure.Email;

public sealed class EmailSettings
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; } = true;
    public string From { get; set; } = default!;
    public string Password { get; set; } = default!;
}
