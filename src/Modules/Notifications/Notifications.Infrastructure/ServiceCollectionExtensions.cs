using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application.Abstractions;
using Notifications.Infrastructure.Email;

namespace Notifications.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModuleNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("Email"));

        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}