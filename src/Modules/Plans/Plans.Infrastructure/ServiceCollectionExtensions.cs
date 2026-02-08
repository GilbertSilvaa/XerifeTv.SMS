using BuildingBlocks.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plans.Application.Contracts;
using Plans.Domain;
using Plans.Infrastructure.Persistence;
using Plans.Infrastructure.Persistence.Database;
using Plans.Infrastructure.Persistence.Repositories;

namespace Plans.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModulePlanInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IPlansReadRepository, PlansReadRepository>();
        services.Decorate<IPlansReadRepository, CachedPlansReadRepository>();
        services.AddScoped<IUnitOfWork<Plan>, PlanUnitOfWork>();

        services.AddDbContextFactory<PlanDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection"), npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(PlanDbContext).Assembly.FullName);
            });
        });

        return services;
    }
}