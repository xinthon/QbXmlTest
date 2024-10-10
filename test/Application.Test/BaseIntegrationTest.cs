using Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QbXmlTest.Test;

public class BaseIntegrationTest
{
    protected IServiceProvider Services;
    public BaseIntegrationTest()
    {
        var services = new ServiceCollection();

        OnConfigureDefaultServices(services);
        OnConfigureServices(services);

        Services = services.BuildServiceProvider();
    }

    protected virtual void OnConfigureServices(IServiceCollection services)
    {
    }

    private void OnConfigureDefaultServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddLogging();
    }
}
