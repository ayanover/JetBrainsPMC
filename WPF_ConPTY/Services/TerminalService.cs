using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ConPTY;
using WPF_ConPTY.Services.Interfaces;

namespace WPF_ConPTY.Services
{
    public class TerminalService : ITerminalService
    {
        private readonly Terminal _terminal;
        private readonly VT100Formatter _formatter;
        private CancellationTokenSource _readCancellation;


        public TerminalService(VT100Formatter formatter)
        {
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _terminal = new Terminal();

            _terminal.OutputReady += Terminal_OutputReady;
        }

        public Task StartTerminalAsync(string command, int width = 80, int height = 120)
        {
            return Task.Run(() => {
                try
                {
                    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    string targetDirectory = currentDirectory;
                    while (!string.IsNullOrEmpty(targetDirectory) &&
                           !Path.GetFileName(targetDirectory.TrimEnd(Path.DirectorySeparatorChar)).Equals("WPF_ConPTY", StringComparison.OrdinalIgnoreCase))
                    {
                        targetDirectory = Path.GetDirectoryName(targetDirectory);
                    }

                    if (string.IsNullOrEmpty(targetDirectory))
                    {
                        targetDirectory = currentDirectory;
                    }
                    Directory.SetCurrentDirectory(targetDirectory);

                    _terminal.Start(command, width, height);
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        MessageBox.Show($"Error starting terminal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });
        }
        public void SendCommand(string command, bool displayInOutput = true)
        {
            if (string.IsNullOrEmpty(command))
                return;

            try
            {
                if (displayInOutput)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        //_formatter.ProcessText($" {command}\r");
                    });
                }

                if (!command.EndsWith("\r"))
                    command += "\r";

                _terminal.WriteToPseudoConsole(command);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => {
                    MessageBox.Show($"Failed to send command: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        public void CloseTerminal()
        {
            if (_readCancellation != null)
            {
                _readCancellation.Cancel();
            }

            try
            {
                _terminal?.WriteToPseudoConsole("exit\r");
            }
            catch
            {

            }
        }

        private void Terminal_OutputReady(object sender, EventArgs e)
        {
            _readCancellation = new CancellationTokenSource();
            ShowWelcomeMessageAsync();
            StartStreamReading();
        }

        public async Task ShowWelcomeMessageAsync()
        {
            await Task.Delay(1000);

            try
            {
                SendCommand("cls", false);
                await Task.Delay(100);
                string profilePath = ("./NewFolder/Startup.ps1");
                SendCommand($". '{profilePath}'", false);
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in welcome message: {ex.Message}");
            }
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

                                Application.Current.Dispatcher.Invoke(() => {
                                    _formatter.ProcessText(text);
                                });
                            }
                            else
                            {
                                Thread.Sleep(10);
                            }
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
                catch
                {
                }
            }, _readCancellation.Token);
        }
    }
}
