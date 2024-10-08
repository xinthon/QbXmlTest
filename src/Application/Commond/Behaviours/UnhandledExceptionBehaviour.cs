using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Commond.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;
    private readonly IHostEnvironment _env;

    public UnhandledExceptionBehaviour(
        ILogger<TRequest> logger, 
        IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex, 
                "Unhandled exception occurred for request {Name} with details {@Request}. Exception: {ExceptionMessage}",
                requestName, 
                request, 
                ex.Message);

            if (_env.IsDevelopment())
            {
                throw;
            }

            return default(TResponse);
        }
    }
}