using System.Windows; 
using Microsoft.Extensions.DependencyInjection;
using JetBrainsInterviewProject.Services;
using JetBrainsInterviewProject.Interfaces;

namespace JetBrainsInterviewProject
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICommandExecutionService, CommandExecutionService>();
            services.AddSingleton<IOutputFormatter, DefaultOutputFormatter>();
            services.AddSingleton<IDispatcherService, WpfDispatcherService>();
    
            services.AddTransient<CommandWindowViewModel>();
            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}