using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using PSHostApp.Interfaces;
using PSHostApp.Services;
using PSHostApp.ViewModels;

namespace PSHostApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPowerShellService, PowerShellService>();
            services.AddSingleton<ICommandHistoryService, CommandHistoryService>();
            services.AddTransient<TerminalViewModel>();
            services.AddTransient<MainWindow>();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            
            base.OnExit(e);
        }
    }
}