using BuildingBlocks;
using BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;
using OutboxPublisher.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddBuildingBlocks(builder.Configuration);

var options = new RabbitMQConnectionOptions();
builder.Configuration
       .GetSection(RabbitMQConnectionOptions.SectionName)
       .Bind(options);

builder.AddWolverineRabbitMQPublisherConfiguration(options);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();