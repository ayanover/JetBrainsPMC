using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using PSHostApp.Interfaces;
using PSHostApp.Enum;

namespace PSHostApp.Services
{
    /// <summary>
    /// Service for PowerShell command execution
    /// </summary>
    public class PowerShellService : IPowerShellService
    {
        private Runspace _runspace;
        private InitialSessionState _initialSessionState;
        private bool _disposed = false;

        public event EventHandler<ConsoleMessageEventArgs> MessageReceived;

        public PowerShellService()
        {
            // Create initial session state
            _initialSessionState = InitialSessionState.CreateDefault();
        }

        public void Initialize()
        {
            try
            {
                _runspace = RunspaceFactory.CreateRunspace(_initialSessionState);
                _runspace.Open();
                
                OnMessageReceived("PowerShell Terminal Ready.\nType commands and press Enter or click Send to execute.\n", 
                    ConsoleMessageType.Success);
                OnMessageReceived("PS> ", ConsoleMessageType.Prompt);
            }
            catch (Exception ex)
            {
                OnMessageReceived($"Error initializing PowerShell: {ex.Message}", ConsoleMessageType.Error);
            }
        }

        public async Task<string> ExecuteCommandAsync(string command)
        {
            // Return empty string if command is null or whitespace
            if (string.IsNullOrWhiteSpace(command))
                return string.Empty;

            return await Task.Run(() =>
            {
                StringBuilder outputBuilder = new StringBuilder();
                
                try
                {
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _runspace;
                        ps.AddScript(command);

                        ps.Streams.Error.DataAdded += (sender, e) =>
                        {
                            var error = ((PSDataCollection<ErrorRecord>)sender)[e.Index];
                            outputBuilder.AppendLine($"Error: {error.Exception.Message}");
                        };
                        
                        ps.Streams.Warning.DataAdded += (sender, e) =>
                        {
                            var warning = ((PSDataCollection<WarningRecord>)sender)[e.Index];
                            outputBuilder.AppendLine($"Warning: {warning.Message}");
                        };
                        
                        ps.Streams.Information.DataAdded += (sender, e) =>
                        {
                            var info = ((PSDataCollection<InformationRecord>)sender)[e.Index];
                            outputBuilder.AppendLine($"Info: {info.MessageData}");
                        };

                        Collection<PSObject> results = ps.Invoke();

                        if (results.Count > 0)
                        {
                            foreach (PSObject result in results)
                            {
                                if (result != null)
                                {
                                    outputBuilder.AppendLine(result.ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    outputBuilder.AppendLine($"Exception: {ex.Message}");
                }

                return outputBuilder.ToString();
            });
        }

        protected virtual void OnMessageReceived(string message, ConsoleMessageType messageType)
        {
            MessageReceived?.Invoke(this, new ConsoleMessageEventArgs(message, messageType));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_runspace != null)
                    {
                        _runspace.Close();
                        _runspace.Dispose();
                        _runspace = null;
                    }
                }

                _disposed = true;
            }
        }

        ~PowerShellService()
        {
            Dispose(false);
        }
    }
}