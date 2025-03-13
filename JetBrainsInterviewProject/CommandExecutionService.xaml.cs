using System.Diagnostics;
using System.Text;

namespace JetBrainsInterviewProject
{
    public delegate void OutputReceivedHandler(string data);
    public interface ICommandExecutionService
    {
        event OutputReceivedHandler OutputReceived;
        event OutputReceivedHandler ErrorReceived;
        Task<CommandResult> ExecuteCommandAsync(string command);
    }

    public class CommandResult
    {
        public string Output { get; set; } = string.Empty;
        public int ExitCode { get; set; }
        public bool IsSuccess => ExitCode == 0;
    }

    public class CommandExecutionService : ICommandExecutionService
    {
        public event OutputReceivedHandler? OutputReceived;
        public event OutputReceivedHandler? ErrorReceived;

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
                        output.AppendLine(args.Data);
                        OutputReceived?.Invoke(args.Data);
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        error.AppendLine($"ERROR: {args.Data}");
                        ErrorReceived?.Invoke($"ERROR: {args.Data}");
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