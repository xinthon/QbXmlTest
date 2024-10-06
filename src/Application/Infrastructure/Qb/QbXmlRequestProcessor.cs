using Application.Commond.Abstractions.Qb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QBXMLRP2Lib;
using System.Runtime.InteropServices;

namespace Application.Infrastructure.Qb;

internal class QbXmlRequestProcessor : IQbXmlRequestProcessor, IDisposable
{
    private readonly RequestProcessor3 _requestProcessor;
    private readonly ILogger<QbXmlRequestProcessor> _logger;
    private readonly IConfiguration _config;
    private readonly SemaphoreSlim _semaphore;
    private string _ticket;

    public QbXmlRequestProcessor(IConfiguration config, ILogger<QbXmlRequestProcessor> logger)
    {
        _config = config;
        _logger = logger;

        _requestProcessor = new RequestProcessor3();
        _semaphore = new SemaphoreSlim(1, 1);
        _ticket = string.Empty;

        OpenConnection();
    }

    private void OpenConnection()
    {
        try
        {
            _logger.LogInformation("Attempting to connect to QuickBooks...");

            var appId = _config.GetValue<string>("Qb:AppID");
            var appName = _config.GetValue<string>("Qb:AppName");

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appName))
            {
                throw new InvalidOperationException("AppID or AppName is not configured properly.");
            }

            _requestProcessor.OpenConnection(appId, appName);
            _logger.LogInformation("Successfully connected to QuickBooks.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to QuickBooks.");
            throw;
        }
    }

    private void InitSession()
    {
        try
        {
            _logger.LogInformation("Initializing QuickBooks session...");

            var qbFilePath = _config.GetValue<string>("Qb:CompanyFilePath");
            _ticket = _requestProcessor.BeginSession(qbFilePath, QBFileMode.qbFileOpenDoNotCare);
            _logger.LogInformation("QuickBooks session started with ticket: {Ticket}", _ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize QuickBooks session.");
            throw;
        }
    }

    public async Task<string> ProcessAsync(string requestXml, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(requestXml))
        {
            throw new ArgumentNullException(nameof(requestXml), "Request XML cannot be null or empty.");
        }

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (string.IsNullOrEmpty(_ticket))
            {
                InitSession();
            }

            _logger.LogInformation("Processing QuickBooks request.");

            var responseXml = _requestProcessor
                .ProcessRequest(_ticket, requestXml);

            _logger.LogInformation("QuickBooks request processed successfully.");

            return responseXml;
        }
        catch (COMException ex) when (ex.ErrorCode == unchecked((int)0x8004041D))
        {
            _ticket = string.Empty;
            _logger.LogWarning("QuickBooks session has expired (error code 0x8004041D).");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during QuickBooks request processing.");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        if (!string.IsNullOrEmpty(_ticket))
        {
            try
            {
                _logger.LogInformation("Ending QuickBooks session.");
                _requestProcessor.EndSession(_ticket);
                _logger.LogInformation("QuickBooks session ended successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while ending QuickBooks session.");
            }
        }

        try
        {
            _requestProcessor.CloseConnection();
            _logger.LogInformation("QuickBooks connection closed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while closing QuickBooks connection.");
        }

        _semaphore.Dispose();
    }
}

