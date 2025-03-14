using JetBrainsInterviewProject.DTO;
using JetBrainsInterviewProject.Enums;

namespace JetBrainsInterviewProject.Interfaces;

public delegate void OutputReceivedHandler(string text, OutputType type);

public interface ICommandExecutionService
{
    event OutputReceivedHandler OutputReceived;
    Task<CommandResult> ExecuteCommandAsync(string command);
}

