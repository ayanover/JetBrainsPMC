using ConPTY;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WPF_ConPTY;

namespace TerminalPoC
{
    public partial class MainWindow : Window
    {
        private Terminal _terminal;
        private CancellationTokenSource _readCancellation;
        private bool _autoScroll = true;
        private VT100Formatter _formatter;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _formatter = new VT100Formatter(OutputRichTextBox);
            _terminal = new Terminal();

            _terminal.OutputReady += Terminal_OutputReady;
            Task.Run(() => {
                try
                {
                    _terminal.Start("powershell.exe -NoProfile -NoExit -Command \"function prompt { return '> ' }; Set-PSReadLineOption -HistorySaveStyle SaveNothing -ShowToolTips:$false -AddToHistoryHandler { return $false }\"", 120, 30); 
                }
                catch (Exception ex)
                {
                }
            });
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
        private void Terminal_OutputReady(object sender, EventArgs e)
        {
            _readCancellation = new CancellationTokenSource();
            StartStreamReading();

            Dispatcher.Invoke(() => {
                Task.Delay(1000).ContinueWith(_ => {
                    try
                    {
                        string scriptPath = Path.Combine(Path.GetTempPath(), "averlogo.ps1");
                        string scriptContent = @"
Clear-Host
$logo = @'
                        _       _____  __  __  _____ 
     /\                ( )     |  __ \|  \/  |/ ____|
    /  \__   _____ _ __|/ ___  | |__) | \  / | |     
   / /\ \ \ / / _ \ '__| / __| |  ___/| |\/| | |     
  / ____ \ V /  __/ |    \__ \ | |    | |  | | |____ 
 /_/    \_\_/ \___|_|    |___/ |_|    |_|  |_|\_____|
                                                     
 ====================================================
           PMC for Rider ver. 0.2.1 ~ Aver
 ====================================================

.
'@
Write-Output $logo
[Console]::ForegroundColor = 'DarkGray'
";

                        File.WriteAllText(scriptPath, scriptContent);
                        string executeCommand = $@"powershell.exe -NoProfile -ExecutionPolicy Bypass -NoLogo -NonInteractive -File ""{scriptPath}""";
                        _terminal.WriteToPseudoConsole("cls\r\n");
                        Thread.Sleep(100);

                        _terminal.WriteToPseudoConsole(executeCommand + "\r\n");
                        Task.Delay(2000).ContinueWith(__ => {
                            try
                            {
                                if (File.Exists(scriptPath))
                                {
                                    File.Delete(scriptPath);
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        });

                    }
                    catch (Exception ex)
                    {
                    }
                });
            });
        }
        private void StartStreamReading()
        {
            Task.Run(() => {
                byte[] buffer = new byte[4096];
                try
                {
                    if (_terminal.ConsoleOutStream == null)
                    {
                        return;
                    }

                    while (!_readCancellation.IsCancellationRequested)
                    {
                        try
                        {
                            int bytesRead = _terminal.ConsoleOutStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                                Dispatcher.Invoke(() => {
                                    _formatter.ProcessText(text);
                                });
                            }
                            else
                            {
                                Thread.Sleep(10);
                            }
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }, _readCancellation.Token);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                SendCommand();
            }
        }

        private void SendCommand()
        {
            string command = InputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(command))
                return;

            try
            {
                _formatter.ProcessText($" {command}\r\n");
                if (!command.EndsWith("\r\n"))
                    command += "\r\n";

                _terminal.WriteToPseudoConsole(command);
                InputTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send command: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_readCancellation != null)
            {
                _readCancellation.Cancel();
            }
            try
            {
                _terminal?.WriteToPseudoConsole("exit\r\n");
            }
            catch (Exception ex)
            {
            }
        }
    }
}