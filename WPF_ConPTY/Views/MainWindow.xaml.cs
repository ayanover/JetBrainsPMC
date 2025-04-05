using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ConPTY;  
using WPF_ConPTY.Services;
using WPF_ConPTY.ViewModels;

namespace WPF_ConPTY.Views
{
    public partial class MainWindow : Window
    {
        private readonly TerminalViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var formatter = new VT100Formatter(OutputRichTextBox);
            var terminalService = new TerminalService(formatter);
            _viewModel = new TerminalViewModel(terminalService);

            DataContext = _viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitializeTerminalAsync();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _viewModel.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                if (_viewModel.SendCommand.CanExecute(null))
                {
                    _viewModel.SendCommand.Execute(null);
                }
            }
        }
    }
}