using BuildingBlocks;
using OutboxPublisher.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddBuildingBlocks(builder.Configuration);
            
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();