using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PSHostApp.Interfaces;
using PSHostApp.Services;
using PSHostApp.ViewModels;

namespace PSHostApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TerminalViewModel _viewModel;
        private readonly IConsoleService _consoleService;

        public MainWindow(IPowerShellService powerShellService, ICommandHistoryService historyService)
        {
            InitializeComponent();
            
            _consoleService = new WpfConsoleService(OutputRichTextBox);
            
            _viewModel = new TerminalViewModel(powerShellService, _consoleService, historyService);
            
            DataContext = _viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Dispose the view model when closing
            _viewModel.Dispose();
        }

        #region Window Controls
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }
}