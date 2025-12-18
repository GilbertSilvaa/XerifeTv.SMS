using BuildingBlocks;
using IntegrationEventConsumer.Worker;
using Plans.Application;
using Plans.Infrastructure;
using Subscribers.Application;
using Subscribers.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
        .AddBuildingBlocks(builder.Configuration)
        .AddModulePlanInfrastructure(builder.Configuration)
        .AddModulePlanApplication()
        .AddModuleSubscriberInfrastructure(builder.Configuration)
        .AddModuleSubscriberApplication();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
