using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Infrastructure.Workers;

public class DataSyncBackgroundService : IHostedService, IDisposable
{
    private readonly ILogger<DataSyncBackgroundService> _logger;
    private Timer? _timer;
    private bool _isRunning;

    private int executionCount = 0;

    public DataSyncBackgroundService(ILogger<DataSyncBackgroundService> logger)
    {
        _logger = logger;
        _isRunning = false;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_isRunning)
        {
            _logger.LogInformation("Starting data synchronization task.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            _isRunning = true;
        }
        else
        {
            _logger.LogInformation("Data synchronization task is already running.");
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_isRunning)
        {
            _logger.LogInformation("Stopping data synchronization task.");
            _timer?.Change(Timeout.Infinite, 0);
            _isRunning = false;
        }
        else
        {
            _logger.LogInformation("Data synchronization task is not running.");
        }
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _logger.LogInformation("Data synchronization task is executing.");
        try
        {  
            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);

            _logger.LogInformation("Data synchronized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred during data sync: {ex.Message}");
        }
    }
    

    public void Dispose()
    {
        _timer?.Dispose();
    }
}