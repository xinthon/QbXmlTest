using Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Desktop;

public partial class App : System.Windows.Application
{
    private IHost _container;

    public App()
    {
        _container = Host.CreateDefaultBuilder()
            .ConfigureLogging(config =>
            {
                config.AddDebug();
                config.AddConsole();
            })
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile("appsettings.json", false, true);
            })
            .ConfigureServices(ConfigureServices)
            .Build();
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddApplication();
        services.AddInfrastructure();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton(s => new MainWindow()
        {
            DataContext = s.GetRequiredService<MainWindowViewModel>()
        });
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _container
            .StartAsync();

        var rootView = _container.Services
            .GetRequiredService<MainWindow>();
        rootView.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _container
            .StopAsync();
    }
}
