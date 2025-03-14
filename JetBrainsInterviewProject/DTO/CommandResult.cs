namespace JetBrainsInterviewProject.DTO;

public class CommandResult
{
    public string Output { get; set; } = string.Empty;
    public int ExitCode { get; set; }
    public bool IsSuccess => ExitCode == 0;
}