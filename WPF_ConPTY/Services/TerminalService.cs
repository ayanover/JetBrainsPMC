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
    /// <summary>
    /// Service that coordinates between terminal functionality and UI
    /// </summary>
    public class TerminalService : ITerminalService
    {
        private readonly Terminal _terminal;
        private readonly VT100Formatter _formatter;
        private CancellationTokenSource _readCancellation;

        /// <summary>
        /// Creates a new terminal service
        /// </summary>
        public TerminalService(VT100Formatter formatter)
        {
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _terminal = new Terminal();

            _terminal.OutputReady += Terminal_OutputReady;
        }

        /// <summary>
        /// Start the terminal with a command
        /// </summary>
        public Task StartTerminalAsync(string command, int width = 120, int height = 30)
        {
            return Task.Run(() => {
                try
                {
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

        /// <summary>
        /// Send a command to the terminal
        /// </summary>
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

        /// <summary>
        /// Close the terminal
        /// </summary>
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
            catch{

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
                // IMPORTANT - I HOPED IT'LL FIX inconsistent line height in larger inpurs but its fucks things up even more
                // TO CHECK LATER
//                string formatSettings = @"
//$FormatEnumerationLimit = 50
//$host.UI.RawUI.BufferSize = New-Object System.Management.Automation.Host.Size 120, 5000
//$PSDefaultParameterValues['Format-Table:AutoSize'] = $true

//function Format-DirectoryListing {
//    param($Path = '.')
//    Get-ChildItem $Path | Out-String -Width 120
//}

//Set-Alias -Name ls -Value Format-DirectoryListing -Option AllScope -Force
//Set-Alias -Name dir -Value Format-DirectoryListing -Option AllScope -Force
//";

//                SendCommand(formatSettings, false);

                string scriptPath = Path.Combine(Path.GetTempPath(), "logo.ps1");
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
           Terminal App v1.0.0
 ====================================================

.
'@
Write-Output $logo
[Console]::ForegroundColor = 'DarkGray'
";

                File.WriteAllText(scriptPath, scriptContent);
                string executeCommand = $@"powershell.exe -NoProfile -ExecutionPolicy Bypass -NoLogo -NonInteractive -File ""{scriptPath}""";
                SendCommand("cls", false);
                await Task.Delay(100);

                SendCommand(executeCommand, false);

                await Task.Delay(2000);

                try
                {
                    if (File.Exists(scriptPath))
                    {
                        File.Delete(scriptPath);
                    }
                }
                catch
                {
                }
            }
            catch
            {
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
