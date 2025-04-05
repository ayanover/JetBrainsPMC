using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using PSHostApp.Interfaces;
using PSHostApp.Enum;
using System.IO;
using System.Reflection;

namespace PSHostApp.Services
{
    public class PowerShellService : IPowerShellService
    {
        private Runspace _runspace;
        private InitialSessionState _initialSessionState;
        private bool _disposed = false;

        public event EventHandler<ConsoleMessageEventArgs> MessageReceived;

        public PowerShellService()
        {
            _initialSessionState = InitialSessionState.CreateDefault();
            _initialSessionState.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Bypass;
        }

        public void Initialize()
        {
            try
            {
                _runspace = RunspaceFactory.CreateRunspace(_initialSessionState);
                _runspace.Open();
                
                SetWorkingDirectoryToProject();
                SetExecutionPolicy();
                LoadNuGetToolsModule();
                
                OnMessageReceived("PowerShell Terminal Ready.\r\nType commands and press Enter or click Send to execute.\r\n", 
                    ConsoleMessageType.Success);
                OnMessageReceived("\r\nPS> ", ConsoleMessageType.Prompt);
            }
            catch (Exception ex)
            {
                OnMessageReceived($"Error initializing PowerShell: {ex.Message}", ConsoleMessageType.Error);
            }
        }

        private void SetExecutionPolicy()
        {
            try
            {
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = _runspace;
                    
                    // Use -Scope Process to only affect this PowerShell session
                    ps.AddScript("Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process -Force");
                    ps.Invoke();
                    
                    // Verify the execution policy was set
                    ps.Commands.Clear();
                    ps.AddScript("Get-ExecutionPolicy");
                    var result = ps.Invoke();
                    
                    if (result.Count > 0)
                    {
                        OnMessageReceived($"Execution policy set to: {result[0]}", ConsoleMessageType.Normal);
                    }
                }
            }
            catch (Exception ex)
            {
                OnMessageReceived($"Error setting execution policy: {ex.Message}", ConsoleMessageType.Error);
            }
        }

        private void SetWorkingDirectoryToProject()
        {
            try
            {
                string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string projectDirectory = FindProjectDirectory(appDirectory);
                
                if (Directory.Exists(projectDirectory))
                {
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _runspace;
                        ps.AddCommand("Set-Location").AddParameter("Path", projectDirectory);
                        ps.Invoke();
                        
                        OnMessageReceived($"Working directory set to: {projectDirectory}", 
                            ConsoleMessageType.Normal);
                    }
                }
                else
                {
                    OnMessageReceived($"Warning: Could not find project directory from {appDirectory}", 
                        ConsoleMessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                OnMessageReceived($"Error setting working directory: {ex.Message}", 
                    ConsoleMessageType.Error);
            }
        }

        private string FindProjectDirectory(string startingDirectory)
        {
            try
            {
                string directory = startingDirectory;
                
                while (directory != null)
                {
                    string parentDir = Directory.GetParent(directory)?.FullName;
                    if (parentDir == null)
                        break;
                    
                    string dirName = new DirectoryInfo(directory).Name.ToLower();
                    
                    if (dirName == "bin" || dirName == "obj")
                    {
                        return parentDir;
                    }
                    
                    directory = parentDir;
                }
                
                return startingDirectory;
            }
            catch
            {
                return startingDirectory;
            }
        }

private void LoadNuGetToolsModule()
{
    try
    {
        string moduleBasePath = string.Empty;
        
        using (PowerShell ps = PowerShell.Create())
        {
            ps.Runspace = _runspace;
            ps.AddCommand("Get-Location");
            var locationResult = ps.Invoke();
            
            if (locationResult.Count > 0 && locationResult[0] != null)
            {
                moduleBasePath = locationResult[0].ToString();
            }
        }
        
        string modulePath = Path.Combine(moduleBasePath, "Models", "NuGetTools");
        
        OnMessageReceived($"Attempting to load module from: {modulePath}", 
            ConsoleMessageType.Normal);
        
        if (!Directory.Exists(modulePath))
        {
            OnMessageReceived($"Warning: NuGetTools module directory not found at {modulePath}", 
                ConsoleMessageType.Warning);
            return;
        }
        
        using (PowerShell ps = PowerShell.Create())
        {
            ps.Runspace = _runspace;
            
            string cmdletsPath = Path.Combine(modulePath, "cmdlets");
            if (Directory.Exists(cmdletsPath))
            {
                string[] psFiles = Directory.GetFiles(cmdletsPath, "*.ps1");
                foreach (string file in psFiles)
                {
                    OnMessageReceived($"Dot-sourcing file: {file}", ConsoleMessageType.Normal);
                    ps.AddScript($". '{file}'");
                    ps.Invoke();
                    
                    if (ps.Streams.Error.Count > 0)
                    {
                        foreach (var error in ps.Streams.Error)
                        {
                            OnMessageReceived($"Error loading file {file}: {error.Exception.Message}", 
                                ConsoleMessageType.Error);
                        }
                        ps.Streams.Error.Clear();
                    }
                }
                
                // Check if the functions were loaded
                ps.Commands.Clear();
                ps.AddScript("Get-Command -Name Find-NuGetPackage, Install-NuGetPackage, Uninstall-NuGetPackage -ErrorAction SilentlyContinue");
                var commands = ps.Invoke();
                
                if (commands.Count > 0)
                {
                    OnMessageReceived("NuGet commands loaded successfully.", 
                        ConsoleMessageType.Success);
                    
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Available NuGet commands:");
                    foreach (var cmd in commands)
                    {
                        sb.AppendLine($"  {cmd.Properties["Name"].Value}");
                    }
                    OnMessageReceived(sb.ToString(), ConsoleMessageType.Normal);
                }
                else
                {
                    ps.Commands.Clear();
                    string psm1File = Path.Combine(modulePath, "NuGetTools.psm1");
                    if (File.Exists(psm1File))
                    {
                        OnMessageReceived($"Trying to dot-source module script directly: {psm1File}", 
                            ConsoleMessageType.Normal);
                        ps.AddScript($". '{psm1File}'");
                        ps.Invoke();
                        
                        if (ps.Streams.Error.Count > 0)
                        {
                            foreach (var error in ps.Streams.Error)
                            {
                                OnMessageReceived($"Error loading module script: {error.Exception.Message}", 
                                    ConsoleMessageType.Error);
                            }
                        }
                        else
                        {
                            ps.Commands.Clear();
                            ps.AddScript("Get-Command -Name Find-NuGetPackage, Install-NuGetPackage, Uninstall-NuGetPackage -ErrorAction SilentlyContinue");
                            commands = ps.Invoke();
                            
                            if (commands.Count > 0)
                            {
                                OnMessageReceived("NuGet commands loaded successfully via direct dot-sourcing.", 
                                    ConsoleMessageType.Success);
                                
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine("Available NuGet commands:");
                                foreach (var cmd in commands)
                                {
                                    sb.AppendLine($"  {cmd.Properties["Name"].Value}");
                                }
                                OnMessageReceived(sb.ToString(), ConsoleMessageType.Normal);
                            }
                            else
                            {
                                OnMessageReceived("Failed to load NuGet commands.",
                                    ConsoleMessageType.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                OnMessageReceived($"Cmdlets directory not found: {cmdletsPath}", 
                    ConsoleMessageType.Warning);
            }
        }
    }
    catch (Exception ex)
    {
        OnMessageReceived($"Exception loading NuGetTools module: {ex.Message}", 
            ConsoleMessageType.Error);
        OnMessageReceived($"Stack trace: {ex.StackTrace}", ConsoleMessageType.Normal);
    }
}
        public async Task<string> ExecuteCommandAsync(string command)
        {
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