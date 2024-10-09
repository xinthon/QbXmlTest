using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviours;

public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly int _maxRetryAttempts = 3;
    private readonly TimeSpan _delay = TimeSpan.FromSeconds(2);

    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;

    public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        int retryAttempts = 0;

        while (true)
        {
            try
            {
                return await next();
            }
            catch (Exception ex) when (retryAttempts < _maxRetryAttempts)
            {
                retryAttempts++;
                var requestName = typeof(TRequest).Name;

                _logger.LogWarning(ex, 
                    "Request handling failed for {RequestType}. Retry attempt {RetryAttempt}/{MaxAttempts}.", 
                    requestName, 
                    retryAttempts, 
                    _maxRetryAttempts);

                await Task.Delay(_delay);
            }
        }
    }
}
