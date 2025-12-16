using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Subscribers.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModuleSubscriberApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        return services;
    }
}