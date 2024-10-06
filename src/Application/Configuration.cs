using Application.Commond.Behaviours;
using Application.Infrastructure.Qb;
using Application.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Application.Commond.Abstractions.Qb;
using Application.Commond.Abstractions.Services;
using FluentValidation;
using Application.Common.Behaviours;

namespace Application;

public static class Configuration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Configuration).Assembly);
        services.AddMediatR(options =>
        {
            options.RegistrationTimeout = 10000; 
            options.RegisterServicesFromAssembly(typeof(Configuration).Assembly);
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
            options.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
            options.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            options.AddOpenBehavior(typeof(RetryBehavior<,>));
        });

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IQbXmlRequestProcessor, QbXmlRequestProcessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}
