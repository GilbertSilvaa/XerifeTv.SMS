using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Core.Messaging.Outbox;
using BuildingBlocks.Infrastructure.Messaging.Dispatchers;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence;
using BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence;
using BuildingBlocks.Infrastructure.Messaging.Publishers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plans.Application.Queries.ReadModels;
using Plans.Domain;
using Plans.Infrastructure.Persistence;
using Plans.Infrastructure.Persistence.Database;
using Plans.Infrastructure.Persistence.Repositories;

namespace Plans.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModulePlanInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPlansRepository, PlansRepository>();
        services.AddScoped<IPlansReadRepository, PlansReadRepository>();
        services.Decorate<IPlansReadRepository, CachedPlansReadRepository>();
        services.AddScoped<IUnitOfWork<Plan>, PlanUnitOfWork>();
        services.AddScoped<PlanCacheInvalidationInterceptor>();

        services.AddScoped<IOutboxMessageDispatcher<Plan>, OutboxMessageDispatcher<Plan>>();
        services.AddScoped<IIntegrationEventPublisher<Plan>, OutboxIntegrationEventPublisher<Plan>>();
        services.AddScoped<IOutboxRepository<Plan>, OutboxRepository<Plan, PlanDbContext>>();
        services.AddScoped<IInboxRepository<Plan>, InboxRepository<Plan, PlanDbContext>>();

        services.AddDbContext<PlanDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection"), npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(PlanDbContext).Assembly.FullName);
            });

            options.AddInterceptors(serviceProvider.GetRequiredService<PlanCacheInvalidationInterceptor>());
        });

        return services;
    }
}