using BuildingBlocks.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Subscribers.Domain;
using Subscribers.Domain.Entities;
using Subscribers.Infrastructure.Persistence;
using Subscribers.Infrastructure.Persistence.Database;
using Subscribers.Infrastructure.Persistence.Repositories;

namespace Subscribers.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModuleSubscriberInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISubscriberRepository, SubscriberRepository>();
        services.AddScoped<IUnitOfWork<Subscriber>, SubscriberUnitOfWork>();

        services.AddDbContextFactory<SubscriberDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection"), npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(SubscriberDbContext).Assembly.FullName);
            });
        });

        return services;
    }
}