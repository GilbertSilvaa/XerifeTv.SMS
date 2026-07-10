using BuildingBlocks;
using BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;
using Identity.Infrastructure;
using OutboxPublisher.Worker;
using Plans.Infrastructure;
using Subscribers.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddModuleIdentityInfrastructure(builder.Configuration)
    .AddModulePlanInfrastructure(builder.Configuration)
    .AddModuleSubscriberInfrastructure(builder.Configuration)
    .AddBuildingBlocks(builder.Configuration);

var options = new RabbitMQConnectionOptions();
builder.Configuration
       .GetSection(RabbitMQConnectionOptions.SectionName)
       .Bind(options);

builder.AddWolverineRabbitMQPublisherConfiguration(options);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();