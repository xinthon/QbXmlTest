using Application.Common.Abstractions.Qb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QbSync.QbXml;
using QbSync.QbXml.Objects;
using QBXMLRP2Lib;
using System.Runtime.InteropServices;

namespace Application.Infrastructure.QuickBooksIntegration;

internal class QuickBooksXmlService : IQuickBooksXmlService, IDisposable
{
    private readonly RequestProcessor3 _requestProcessor;
    private readonly ILogger<QuickBooksXmlService> _logger;
    private readonly IConfiguration _config;
    private readonly SemaphoreSlim _semaphore;

    private readonly string _qbFilePath;
    private readonly string _appId;
    private readonly string _appName;

    private string _ticket = string.Empty;

    public QuickBooksXmlService(IConfiguration config, ILogger<QuickBooksXmlService> logger)
    {
        _config = config;
        _logger = logger;

        _requestProcessor = new RequestProcessor3();
        _semaphore = new SemaphoreSlim(1, 1);

        _qbFilePath = _config.GetValue<string>("Qb:CompanyFilePath") ??
            throw new ArgumentNullException("Qb:CompanyFilePath is not configured");

        _appId = _config.GetValue<string>("Qb:AppID") ??
            throw new ArgumentNullException("Qb:AppID is not configured");

        _appName = _config.GetValue<string>("Qb:AppName") ??
            throw new ArgumentNullException("Qb:AppName is not configured");

        OpenConnection();
    }

    private void OpenConnection()
    {
        try
        {
            _logger.LogInformation("Attempting to connect to QuickBooks...");
            _requestProcessor.OpenConnection(_appId, _appName);
            _logger.LogInformation("Successfully connected to QuickBooks.");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to QuickBooks.");
            throw;
        }
    }

    public async Task<string> SendRequestAsync(string requestXml, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrEmpty(requestXml))
            throw new ArgumentNullException(nameof(requestXml), "Request XML cannot be null or empty.");

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Initializing QuickBooks session...");
            _ticket = _requestProcessor.BeginSession(_qbFilePath, QBXMLRP2Lib.QBFileMode.qbFileOpenDoNotCare);
            _logger.LogInformation("QuickBooks session started with ticket: {Ticket}", _ticket);

            _logger.LogInformation("Processing QuickBooks request.");
            var responseXml = await Task.Run(() => _requestProcessor.ProcessRequest(_ticket, requestXml), cancellationToken);
            _logger.LogInformation("QuickBooks request processed successfully.");

            return responseXml;
        }
        catch (COMException ex) when (ex.ErrorCode == unchecked((int)0x8004041D))
        {
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
            if (!string.IsNullOrEmpty(_ticket))
            {
                _logger.LogInformation("Ending QuickBooks session.");
                _requestProcessor.EndSession(_ticket);
                _logger.LogInformation("QuickBooks session ended successfully.");
            }
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        try
        {
            _requestProcessor.CloseConnection();
            _logger.LogInformation("QuickBooks connection closed.");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error occurred while closing QuickBooks connection.");
        }
        _semaphore.Dispose();
    }
}

