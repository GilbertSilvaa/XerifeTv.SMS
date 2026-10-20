using BuildingBlocks;
using BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;
using Identity.Infrastructure;
using OutboxPublisher.Worker;
using Payments.Infrastructure;
using Payments.Infrastructure.PaymentGateway.Stripe;
using Plans.Infrastructure;
using Subscribers.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddModuleIdentityInfrastructure(builder.Configuration)
    .AddModulePlanInfrastructure(builder.Configuration)
    .AddModuleSubscriberInfrastructure(builder.Configuration)
    .AddModulePaymentsInfrastructure(builder.Configuration)
    .AddBuildingBlocks(builder.Configuration);

var options = new RabbitMQConnectionOptions();
builder.Configuration
       .GetSection(RabbitMQConnectionOptions.SectionName)
       .Bind(options);

builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.SectionName));

builder.AddWolverineRabbitMQPublisherConfiguration(options);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();