using BuildingBlocks;
using BuildingBlocks.Infrastructure.Messaging.Buses;
using BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;
using OutboxPublisher.Worker;
using ICoreMessageBus = BuildingBlocks.Core.Messaging.IMessageBus;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddBuildingBlocks(builder.Configuration);

string messageBusProvider = builder.Configuration["MessageBusProvider"] ?? string.Empty;

if (messageBusProvider.Equals("Wolverine", StringComparison.OrdinalIgnoreCase))
{
    builder.AddWolverineRabbitMQPublisherConfiguration(builder.Configuration);
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