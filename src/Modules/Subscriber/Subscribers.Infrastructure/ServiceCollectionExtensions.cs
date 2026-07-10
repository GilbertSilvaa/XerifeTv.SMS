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
using Subscribers.Application.PlanCatalog;
using Subscribers.Application.Queries.ReadModels;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;
using Subscribers.Infrastructure.Persistence;
using Subscribers.Infrastructure.Persistence.Database;
using Subscribers.Infrastructure.Persistence.Repositories;

namespace Subscribers.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModuleSubscriberInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOutboxMessageDispatcher<Subscriber>, OutboxMessageDispatcher<Subscriber>>();
        services.AddScoped<IIntegrationEventPublisher<Subscriber>, OutboxIntegrationEventPublisher<Subscriber>>();
        services.AddScoped<IOutboxRepository<Subscriber>, OutboxRepository<Subscriber, SubscriberDbContext>>();
        services.AddScoped<IInboxRepository<Subscriber>, InboxRepository<Subscriber, SubscriberDbContext>>();

        services.AddScoped<ISubscribersRepository, SubscribersRepository>();
        services.AddScoped<ISubscribersReadRepository, SubscribersReadRepository>();
        services.AddScoped<IPlanCatalogRepository, PlanCatalogRepository>();
        services.AddScoped<IUnitOfWork<Subscriber>, SubscriberUnitOfWork>();

        services.AddDbContext<SubscriberDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection"), npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(SubscriberDbContext).Assembly.FullName);
            });
        });

        return services;
    }
}