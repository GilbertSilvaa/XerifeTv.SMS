using BuildingBlocks.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Plans.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddModulePlanApplication(this IServiceCollection services)
	{
		services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.NotificationPublisherType = typeof(IdempotencyIntegrationEventHandlerBehavior);
        });

		services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

		return services;
	}
}
