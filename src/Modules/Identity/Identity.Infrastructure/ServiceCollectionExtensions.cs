using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Core.Messaging.Outbox;
using BuildingBlocks.Infrastructure.Messaging.Dispatchers;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence;
using BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence;
using BuildingBlocks.Infrastructure.Messaging.Publishers;
using Identity.Infrastructure.Persistence.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Identity.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModuleIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOutboxMessageDispatcher<UserIdentityAggregateRoot>, OutboxMessageDispatcher<UserIdentityAggregateRoot>>();
        services.AddScoped<IIntegrationEventPublisher<UserIdentityAggregateRoot>, OutboxIntegrationEventPublisher<UserIdentityAggregateRoot>>();
        services.AddScoped<IOutboxRepository<UserIdentityAggregateRoot>, OutboxRepository<UserIdentityAggregateRoot, IdentityDbContext>>();
        services.AddScoped<IInboxRepository<UserIdentityAggregateRoot>, InboxRepository<UserIdentityAggregateRoot, IdentityDbContext>>();
        services.AddScoped<IUnitOfWork<UserIdentityAggregateRoot>, UserIdentityAggregateRootUnitOfWork>();

        services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection"), npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
            });
        });

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();

        var jwtSettingsSection = configuration.GetSection("Jwt");
        if (jwtSettingsSection.Exists()) services.AddSettingJWT(configuration);

        return services;
    }

    private static IServiceCollection AddSettingJWT(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        byte[] key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = JwtTokenService.GetTokenValidationParameters(configuration);
            });

        services.AddAuthorization();

        return services;
    }
}