using BuildingBlocks;
using Identity.API;
using Identity.Application;
using Identity.Infrastructure;
using Microsoft.OpenApi.Models;
using Plans.API;
using Plans.Application;
using Plans.Infrastructure;
using Subscribers.API;
using Subscribers.Application;
using Subscribers.Infrastructure;
using WebAPI.Host;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddProblemDetails(opts =>
        {
            opts.CustomizeProblemDetails = context =>
            {
                if (context.Exception is not null)
                {
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                    context.ProblemDetails.Extensions["Timestamp"] = DateTime.UtcNow;
                    context.ProblemDetails.Instance = context.HttpContext.Request.Path;
                }
            };
        });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // Add services to the container.
        builder.Services
            .AddBuildingBlocks(builder.Configuration)
            .AddModulePlanInfrastructure(builder.Configuration)
            .AddModulePlanApplication()
            .AddModuleIdentityInfrastructure(builder.Configuration)
            .AddModuleIdentityApplication()
            .AddModuleSubscriberInfrastructure(builder.Configuration)
            .AddModuleSubscriberApplication();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter only the JWT token (without 'Bearer')"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapPlansEndpoints();
        app.MapIdentityEndpoints();
        app.MapSubscribersEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.Run();
    }
}