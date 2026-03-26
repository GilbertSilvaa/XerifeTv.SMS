using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Notifications.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModuleNotificationApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}