using System;
using System.Diagnostics;
using System.Text;
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
        private static readonly SolidColorBrush StdoutColor = new SolidColorBrush(Colors.PaleGreen);
        private static readonly SolidColorBrush StderrColor = new SolidColorBrush(Colors.OrangeRed);
        private static readonly SolidColorBrush InfoColor = new SolidColorBrush(Colors.LightSkyBlue);
        private static readonly SolidColorBrush SuccessColor = new SolidColorBrush(Colors.ForestGreen);
        private static readonly SolidColorBrush WarningColor = new SolidColorBrush(Colors.Yellow);
        private static readonly SolidColorBrush ErrorColor = new SolidColorBrush(Colors.Red);

        public MainWindow()
        {
            InitializeComponent();
            CommandInputTextBox.Focus();
            
            DisplayColoredText("Ready to execute commands. Type a command and press Enter or click Execute.", InfoColor);
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
                

                await Task.Run(() => RunProcess(command));
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

        private void RunProcess(string command)
        {
            string executable = "cmd.exe";
            string arguments = $"/c {command}";

            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            DisplayColoredText(args.Data + Environment.NewLine, StdoutColor);
                        });
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            DisplayColoredText(args.Data + Environment.NewLine, StderrColor);
                        });
                    }
                };
                
                process.Start();
                
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                process.WaitForExit();
                
                Dispatcher.Invoke(() =>
                {
                    if (process.ExitCode == 0)
                    {
                        StatusText.Text = "Command executed successfully";
                        DisplayColoredText($"\nCommand completed with exit code: {process.ExitCode}\n", SuccessColor);
                    }
                    else
                    {
                        StatusText.Text = $"Command exited with code {process.ExitCode}";
                        DisplayColoredText($"\nCommand completed with exit code: {process.ExitCode}\n", 
                            process.ExitCode > 0 ? WarningColor : ErrorColor);
                    }
                });
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