using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure.Email;
using Notifications.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence;
using Notifications.Infrastructure.Email.Abstractions;

namespace Notifications.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModuleNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("Email"));

        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddScoped<IUnitOfWork<NotificationAggregateRoot>, NotificationAggregationRootUnitOfWork>();
        services.AddScoped<IInboxRepository<NotificationAggregateRoot>, InboxRepository<NotificationAggregateRoot, NotificationDbContext>>();

        services.AddDbContext<NotificationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection"), npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName);
            });
        });

        return services;
    }
}