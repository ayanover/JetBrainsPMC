using System.Diagnostics;
using System.Text;
using JetBrainsInterviewProject.Interfaces;
using JetBrainsInterviewProject.Enums;
using JetBrainsInterviewProject.DTO;

namespace JetBrainsInterviewProject.Services
{
    public class CommandExecutionService : ICommandExecutionService
    {
        public event OutputReceivedHandler? OutputReceived;

        public async Task<CommandResult> ExecuteCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException("Command cannot be empty", nameof(command));
            }

            return await Task.Run(() => ExecuteCommand(command));
        }

        private CommandResult ExecuteCommand(string command)
        {
            string executable = "cmd.exe";
            string arguments = $"/c {command}";
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();
            int exitCode = 0;

            using (Process process = new())
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
                        output.AppendLine(args.Data);
                        OutputReceived?.Invoke(args.Data, OutputType.Standard);
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        error.AppendLine(args.Data);
                        OutputReceived?.Invoke(args.Data, OutputType.Error);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                exitCode = process.ExitCode;
            }

            var result = new CommandResult
            {
                Output = output.ToString() + error.ToString(),
                ExitCode = exitCode
            };

            return result;
        }
    }
}