using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ConPTY;
using WPF_ConPTY.Views;
using WPF_ConPTY.Services;
using WPF_ConPTY.Services.Interfaces;
using WPF_ConPTY.ViewModels;

namespace YourApp.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<Terminal>();
            services.AddSingleton<ITerminalService, TerminalService>();
            services.AddSingleton<IVT100Formatter, VT100Formatter>();

            services.AddSingleton<TerminalViewModel>();
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider.Dispose();
            base.OnExit(e);
        }
    }
}