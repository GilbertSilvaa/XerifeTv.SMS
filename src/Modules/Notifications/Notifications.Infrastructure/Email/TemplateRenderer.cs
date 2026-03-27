namespace Notifications.Infrastructure.Email;

public sealed class TemplateRenderer
{
    public static async Task<string> RenderAsync(string templateName, Dictionary<string, string> keyValues)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Email",
            "Templates",
            $"{templateName}.html"
        );

        var template = await File.ReadAllTextAsync(path);

        foreach (var item in keyValues)
            template = template.Replace("{{" + item.Key + "}}", item.Value);

        return template;
    }
}