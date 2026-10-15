using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Payments.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModulePaymentApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        return services;
    }
}