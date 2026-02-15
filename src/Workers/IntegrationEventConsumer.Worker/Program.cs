using BuildingBlocks;
using BuildingBlocks.Infrastructure.Messaging.Buses;
using BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;
using Identity.Application;
using Identity.Infrastructure;
using IntegrationEventConsumer.Worker;
using Subscribers.Application;
using Subscribers.Infrastructure;
using ICoreMessageBus = BuildingBlocks.Core.Messaging.IMessageBus;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
        .AddBuildingBlocks(builder.Configuration)
        .AddModuleSubscriberInfrastructure(builder.Configuration)
        .AddModuleSubscriberApplication()
        .AddModuleIdentityInfrastructure(builder.Configuration)
        .AddModuleIdentityApplication();

string messageBusProvider = builder.Configuration["MessageBusProvider"] ?? string.Empty;

if (messageBusProvider.Equals("Wolverine", StringComparison.OrdinalIgnoreCase))
{
    builder.AddWolverineRabbitMQConsumerConfiguration(builder.Configuration);
    builder.Services.AddSingleton<ICoreMessageBus, WolverineRabbitMQMessageBus>();
}
else
{
    builder.Services.AddSingleton(provider => new RabbitMqConnectionProvider(builder.Configuration));
    builder.Services.AddSingleton<ICoreMessageBus, RabbitMQMessageBus>();
}

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
