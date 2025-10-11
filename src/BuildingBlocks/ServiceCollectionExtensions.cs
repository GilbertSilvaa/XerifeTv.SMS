using BuildingBlocks.Behaviors;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddBuildingBlocks(this IServiceCollection services, IConfiguration configuration)
	{
		services
			.AddBuildingBlocksInfrastructure(configuration)
			.AddBuildingBlocksBehaviors()
			.AddBuildingBlocksMapping();

		return services;
	}

	private static IServiceCollection AddBuildingBlocksMapping(this IServiceCollection services)
	{
		var config = TypeAdapterConfig.GlobalSettings;
		config.Scan(AppDomain.CurrentDomain.GetAssemblies());
		services.AddSingleton(config);

		return services;
	}

	private static IServiceCollection AddBuildingBlocksInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddScoped<IDomainEventPublisher, MediaRDomainEventPublisher>();
		services.AddScoped<IIntegrationEventPublisher, MediaRIntegrationEventPublisher>();

		services.AddStackExchangeRedisCache(options =>
		{
			options.Configuration = configuration["Redis:Connection"];
			options.InstanceName = configuration["Redis:InstanceName"];
		});

		return services;
	}

	private static IServiceCollection AddBuildingBlocksBehaviors(this IServiceCollection services)
	{
		services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
		return services;
	}
}
