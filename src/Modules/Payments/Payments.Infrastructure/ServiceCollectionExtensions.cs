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
using Payments.Application.Abstractions;
using Payments.Domain.Customer;
using Payments.Domain.PaymentSetup;
using Payments.Infrastructure.PaymentGateway.Stripe;
using Payments.Infrastructure.PaymentGateway.Stripe.StripeEventTranslators;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence.Database;
using Payments.Infrastructure.Persistence.Repositories;
using StackExchange.Redis;
using StripeLib = Stripe;

namespace Payments.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModulePaymentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOutboxMessageDispatcher<Customer>, OutboxMessageDispatcher<Customer>>();
        services.AddScoped<IIntegrationEventPublisher<Customer>, OutboxIntegrationEventPublisher<Customer>>();
        services.AddScoped<IOutboxRepository<Customer>, OutboxRepository<Customer, PaymentDbContext>>();
        services.AddScoped<IInboxRepository<Customer>, InboxRepository<Customer, PaymentDbContext>>();

        services.AddScoped<IOutboxMessageDispatcher<PaymentSetup>, OutboxMessageDispatcher<PaymentSetup>>();
        services.AddScoped<IIntegrationEventPublisher<PaymentSetup>, OutboxIntegrationEventPublisher<PaymentSetup>>();
        services.AddScoped<IOutboxRepository<PaymentSetup>, OutboxRepository<PaymentSetup, PaymentDbContext>>();
        services.AddScoped<IInboxRepository<PaymentSetup>, InboxRepository<PaymentSetup, PaymentDbContext>>();

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUnitOfWork<Customer>, PaymentUnitOfWorks.CustomerUnitOfWork>();
        services.AddScoped<IPaymentSetupRepository, PaymentSetupRepository>();
        services.AddScoped<IUnitOfWork<PaymentSetup>, PaymentUnitOfWorks.PaymentSetupUnitOfWork>();

        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddSingleton<StripeLib.IStripeClient, StripeLib.StripeClient>(serviceProvider =>
        {
            var stripeOptions = configuration.GetSection(StripeOptions.SectionName).Get<StripeOptions>();
            return new StripeLib.StripeClient(stripeOptions?.ApiKey);
        });

        services.AddDbContext<PaymentDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection"), npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(PaymentDbContext).Assembly.FullName);
            });
        });

        services.AddSingleton<IPaymentGatewayEventToIntegrationEventTranslator, CheckoutSessionCompletedTranslator>();
        services.AddSingleton<IPaymentGatewayEventToIntegrationEventTranslator, PaymentMethodAttachedTranslator>();
        services.AddSingleton<IPaymentGatewayEventToIntegrationEventTranslator, CheckoutSessionExpiredTranslator>();

        return services;
    }
}
