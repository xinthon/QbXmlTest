using Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private IHost _container;

        public App()
        {
            _container = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(options => options.AddJsonFile("appsettings.json", false, true))
                .ConfigureServices(ConfigureServices)
                .Build();
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton(s => new MainWindow()
            {
                DataContext = s.GetRequiredService<MainWindowViewModel>()
            });

            services.AddApplication();
            services.AddInfrastructure();
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
}
