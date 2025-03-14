using System.Windows.Media;
using JetBrainsInterviewProject.DTO;
using JetBrainsInterviewProject.Enums;

namespace JetBrainsInterviewProject.Interfaces;

public interface IOutputFormatter
{
    (string FormattedText, SolidColorBrush Color) FormatOutput(string text, OutputType type);
    (string FormattedText, SolidColorBrush Color) FormatCommandExecutionStart(string command);
    (string FormattedText, SolidColorBrush Color) FormatCommandResult(CommandResult result);
    (string FormattedText, SolidColorBrush Color) FormatError(string errorMessage);
}