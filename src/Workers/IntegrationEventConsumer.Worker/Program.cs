using BuildingBlocks;
using BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;
using Identity.Application;
using Identity.Infrastructure;
using IntegrationEventConsumer.Worker;
using Notifications.Application;
using Notifications.Infrastructure;
using Subscribers.Application;
using Subscribers.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
        .AddBuildingBlocks(builder.Configuration)
        .AddModuleSubscriberInfrastructure(builder.Configuration)
        .AddModuleSubscriberApplication()
        .AddModuleIdentityInfrastructure(builder.Configuration)
        .AddModuleIdentityApplication()
        .AddModuleNotificationInfrastructure(builder.Configuration)
        .AddModuleNotificationApplication();

var options = new RabbitMQConnectionOptions();
builder.Configuration
       .GetSection(RabbitMQConnectionOptions.SectionName)
       .Bind(options);

builder.AddWolverineRabbitMQConsumerConfiguration(options);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
