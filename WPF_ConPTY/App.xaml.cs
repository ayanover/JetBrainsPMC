using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace GUIConsole.Wpf
{
    public partial class App : Application
    {
        public App()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"Unhandled exception: {e.Exception}");
            
            if (e.Exception.Message.Contains("resources") || 
                e.Exception.Message.Contains("resource"))
            {
                e.Handled = true;
            }
            else
            {
                MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
        }
    }
}