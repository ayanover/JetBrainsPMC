using System;
using Microsoft.Extensions.DependencyInjection;
using WPF_ConPTY.Services;
using WPF_ConPTY.ViewModels;
using WPF_ConPTY.Views;
using ConPTY;
using WPF_ConPTY.Services.Interfaces;
using WPF_ConPTY.Services;
using WPF_ConPTY.ViewModels;

namespace WPF_ConPTY.Extensions
{
    /// <summary>
    /// Extension methods for configuring services
    /// </summary>
    public static class StartupExtensions
    {
        /// <summary>
        /// Configures terminal services
        /// </summary>
        public static IServiceCollection AddTerminalServices(this IServiceCollection services)
        {
            // Register Terminal from ConPTY
            services.AddSingleton<Terminal>();

            // Register services
            services.AddSingleton<ITerminalService, TerminalService>();

            return services;
        }

        /// <summary>
        /// Configures the formatter services
        /// </summary>
        public static IServiceCollection AddFormatterServices(this IServiceCollection services)
        {
            // Register formatter as transient because it needs a RichTextBox instance
            // which is created as part of the UI
            services.AddTransient<IVT100Formatter, VT100Formatter>();

            return services;
        }

        /// <summary>
        /// Configures command interceptor services
        /// </summary>
        public static IServiceCollection AddCommandInterceptorServices(this IServiceCollection services)
        {
            // HTTP client factory for NuGet API
            services.AddHttpClient("NuGet", client =>
            {
                client.BaseAddress = new Uri("https://azuresearch-usnc.nuget.org/");
                client.DefaultRequestHeaders.Add("User-Agent", "YourApp Terminal");
            });

            // Command interceptor
            services.AddSingleton<ICommandInterceptor, CommandInterceptor>();

            // Command handlers
            services.AddSingleton<INuGetCommandHandler, NuGetCommandHandler>();

            return services;
        }

        /// <summary>
        /// Configures view models
        /// </summary>
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddSingleton<TerminalViewModel>();

            return services;
        }

        /// <summary>
        /// Configures views
        /// </summary>
        public static IServiceCollection AddViews(this IServiceCollection services)
        {
            services.AddTransient<MainWindow>();

            return services;
        }
    }
}