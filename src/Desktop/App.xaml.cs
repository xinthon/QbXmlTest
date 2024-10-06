using Application;
using Desktop.Framework;
using Desktop.ViewModels;
using Desktop.ViewModels.Components;
using Desktop.ViewModels.Dialogs;
using Desktop.Views;
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

        // Frameworkd
        services.AddSingleton<ViewModelFactory>();
        services.AddSingleton<DialogManager>();
        services.AddSingleton<ViewManager>();
        services.AddSingleton<ViewModelManager>();


        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<TableViewModel>();
        services.AddTransient<MessageBoxViewModel>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _container
            .StartAsync();

        var rootView = new MainView()
        {
            DataContext = _container
                .Services
                .GetRequiredService<ViewModelFactory>()
                .Create<MainViewModel>()
        };
        rootView.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _container
            .StopAsync();
    }
}
