using BuildingBlocks.Behaviors;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Outbox;
using BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;
using BuildingBlocks.Infrastructure.Messaging.Dispatchers;
using BuildingBlocks.Infrastructure.Messaging.Publishers;
using BuildingBlocks.Infrastructure.Outbox.Persistence;
using BuildingBlocks.Infrastructure.Outbox.Persistence.Database;
using BuildingBlocks.IntegrationEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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
		services.AddScoped<IDomainEventDispatcher, MediaRDomainEventDispatcher>();
        services.AddScoped<IIntegrationEventDispatcher, MediaRIntegrationEventDispatcher>();
        services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();
		services.AddScoped<IOutboxRepository, OutboxRepository>();

		services.AddSingleton<IntegrationEventTypeMapper>();

		services.AddSingleton(provider => new RabbitMqConnectionProvider(configuration));
		services.AddSingleton<IMessageBus, RabbitMQMessageBus>();

		services.AddStackExchangeRedisCache(options =>
		{
			options.Configuration = configuration["Redis:Connection"];
			options.InstanceName = configuration["Redis:InstanceName"];
		});

		services.AddDbContextFactory<OutboxDbContext>((serviceProvider, options) =>
		{
			options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection"), npgsqlOptions =>
			{
				npgsqlOptions.MigrationsAssembly(typeof(OutboxDbContext).Assembly.FullName);
			});
		});

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        return services;
	}

	private static IServiceCollection AddBuildingBlocksBehaviors(this IServiceCollection services)
	{
		services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
		return services;
	}
}
