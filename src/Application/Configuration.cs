using Application.Common.Behaviours;
using Application.Infrastructure.QuickBooksIntegration;
using Application.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Abstractions.Qb;
using Application.Common.Abstractions.Services;
using FluentValidation;
using Application.Infrastructure.Workers;
using QbSync.QbXml;
using QbSync.QbXml.Objects;
using Application.Features.Qb.Queries.GetQbList;
using MediatR;
using System.Reflection;

namespace Application;

/// <summary>
/// Configuration class for registering application services and behaviors.
/// </summary>
public static class Configuration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register validators from the current assembly
        services.AddValidatorsFromAssembly(typeof(Configuration).Assembly);

        // Register QB request/response services from the QB XML assembly
        services.RegisterQbRequestResponseServices(typeof(QbXmlRequest).Assembly);

        // Register MediatR with common behaviors like validation, performance, retry, and exception handling
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(Configuration).Assembly);

            // Validation should come first to catch any invalid input early.
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));

            // Performance tracking before handling retries and exceptions.
            options.AddOpenBehavior(typeof(PerformanceBehaviour<,>));

            // Retry logic in case the operation can be retried.
            options.AddOpenBehavior(typeof(RetryBehavior<,>));

            // Catch unhandled exceptions after all retries are exhausted.
            options.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
        });

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddHostedService<DataSyncBackgroundService>();
        services.AddScoped<IQuickBooksXmlService, QuickBooksXmlService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }

    /// <summary>
    /// Registers QB XML request and response service pairs from the specified assembly.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="assembly">The assembly containing QB request and response types.</param>
    public static void RegisterQbRequestResponseServices(this IServiceCollection services, Assembly assembly)
    {
        // Retrieve all types from the assembly
        var assemblyTypes = assembly.GetTypes();

        // Filter and group types based on their corresponding request or response type
        var qbRequestResponseGroups = assemblyTypes
            .Where(type => typeof(IQbRequest).IsAssignableFrom(type) || typeof(IQbResponse).IsAssignableFrom(type))
            .GroupBy(type =>
            {
                var typeName = type.FullName ?? type.Name;

                // Identify the base name of the QB type, removing "RqType" or "RsType" suffixes
                if (typeName.EndsWith("RqType"))
                    return typeName.Replace("RqType", "");

                if (typeName.EndsWith("RsType"))
                    return typeName.Replace("RsType", "");

                return typeName;
            })
            .ToList();

        // Iterate over each group to register the services
        foreach (var qbTypeGroup in qbRequestResponseGroups)
        {
            var qbRequestType = qbTypeGroup
                .FirstOrDefault(type => typeof(IQbRequest).IsAssignableFrom(type));
            var qbResponseType = qbTypeGroup
                .FirstOrDefault(type => typeof(IQbResponse).IsAssignableFrom(type));

            // Register services only if both request and response types are found in the group
            if (qbRequestType != null && qbResponseType != null)
            {
                services.RegisterQbRequestResponsePair(qbRequestType, qbResponseType);
            }
        }
    }

    /// <summary>
    /// Registers a QB request and its corresponding response into the service collection.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="qbRequestType">The type representing a QB request.</param>
    /// <param name="qbResponseType">The type representing a QB response.</param>
    public static void RegisterQbRequestResponsePair(this IServiceCollection services, System.Type qbRequestType, System.Type qbResponseType)
    {
        // Create a generic type for GetQbListQuery<TRequest, TResponse>
        var qbListQueryType = typeof(GetQbListQuery<,>).MakeGenericType(qbRequestType, qbResponseType);

        // Create a generic type for GetQbListQueryHandler<TRequest, TResponse>
        var qbListQueryHandlerType = typeof(GetQbListQueryHandler<,>).MakeGenericType(qbRequestType, qbResponseType);

        // Register the QB request and response service pair in the service collection
        services.AddTransient(
            typeof(IRequest<>).MakeGenericType(qbResponseType),
            qbListQueryType);

        services.AddTransient(
            typeof(IRequestHandler<,>).MakeGenericType(qbListQueryType, qbResponseType),
            qbListQueryHandlerType);
    }
}
