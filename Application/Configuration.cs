using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class Configuration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(Configuration).Assembly);
        });
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}
