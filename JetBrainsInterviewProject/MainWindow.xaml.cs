using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace JetBrainsInterviewProject
{
    public partial class MainWindow : Window
    {
        private static readonly SolidColorBrush StdoutColor = new SolidColorBrush(Colors.LightGray);
        private static readonly SolidColorBrush StderrColor = new SolidColorBrush(Colors.OrangeRed);
        private static readonly SolidColorBrush SuccessColor = new SolidColorBrush(Colors.ForestGreen);
        private static readonly SolidColorBrush WarningColor = new SolidColorBrush(Colors.Yellow);
        private static readonly SolidColorBrush ErrorColor = new SolidColorBrush(Colors.Red);
        
        private readonly ICommandExecutionService _commandService;

        public MainWindow()
        {
            InitializeComponent();
            CommandInputTextBox.Focus();
            
            _commandService = new CommandExecutionService();
            
            ((CommandExecutionService)_commandService).OutputReceived += OnOutputReceived;
            ((CommandExecutionService)_commandService).ErrorReceived += OnErrorReceived;
        }
        
        private void OnOutputReceived(string data)
        {
            Dispatcher.Invoke(() =>
            {
                DisplayColoredText(data + Environment.NewLine, StdoutColor);
            });
        }
        
        private void OnErrorReceived(string data)
        {
            Dispatcher.Invoke(() =>
            {
                DisplayColoredText(data + Environment.NewLine, StderrColor);
            });
        }

        private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteCommand();
        }

        private async void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await ExecuteCommand();
            }
        }

        private async Task ExecuteCommand()
        {
            string command = CommandInputTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(command))
                return;

            try
            {
                CommandInputTextBox.IsEnabled = false;
                ExecuteButton.IsEnabled = false;
                StatusText.Text = "Executing command...";
                
                CommandOutput.Document.Blocks.Clear();
                
                CommandResult result = await _commandService.ExecuteCommandAsync(command);
                
                if (result.IsSuccess)
                {
                    StatusText.Text = "Command executed successfully";
                    DisplayColoredText($"\nCommand completed with exit code: {result.ExitCode}\n", SuccessColor);
                }
                else
                {
                    StatusText.Text = $"Command exited with code {result.ExitCode}";
                    DisplayColoredText($"\nCommand completed with exit code: {result.ExitCode}\n", 
                        result.ExitCode > 0 ? WarningColor : ErrorColor);
                }
            }
            catch (Exception ex)
            {
                DisplayColoredText($"Error executing command: {ex.Message}\n", ErrorColor);
                StatusText.Text = "Command execution failed";
            }
            finally
            {
                CommandInputTextBox.IsEnabled = true;
                ExecuteButton.IsEnabled = true;
                CommandInputTextBox.Focus();
                CommandInputTextBox.SelectAll();
            }
        }

        private void DisplayColoredText(string text, SolidColorBrush color)
        {
            TextRange tr = new TextRange(CommandOutput.Document.ContentEnd, CommandOutput.Document.ContentEnd);
            tr.Text = text;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            CommandOutput.ScrollToEnd();
        }
    }
}