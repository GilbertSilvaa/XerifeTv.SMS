using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Identity.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddModuleIdentityApplication(this IServiceCollection services)
	{
		services.AddMediatR(cfg =>
			cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

		return services;
	}
}
