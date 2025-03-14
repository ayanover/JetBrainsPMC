using System.Windows.Media;
using JetBrainsInterviewProject.DTO;
using JetBrainsInterviewProject.Enums;
using JetBrainsInterviewProject.Interfaces;

namespace JetBrainsInterviewProject;

public class DefaultOutputFormatter : IOutputFormatter
{
    public (string FormattedText, SolidColorBrush Color) FormatOutput(string text, OutputType type)
    {
        var color = type == OutputType.Standard
            ? new SolidColorBrush(Colors.PaleGreen)
            : new SolidColorBrush(Colors.OrangeRed);

        return (text + Environment.NewLine, color);
    }

    public (string FormattedText, SolidColorBrush Color) FormatCommandExecutionStart(string command)
    {
        return ($"> {command}\n", new SolidColorBrush(Colors.LightSkyBlue));
    }

    public (string FormattedText, SolidColorBrush Color) FormatCommandResult(CommandResult result)
    {
        var color = result.IsSuccess
            ? new SolidColorBrush(Colors.ForestGreen)
            : result.ExitCode > 0
                ? new SolidColorBrush(Colors.Yellow)
                : new SolidColorBrush(Colors.Red);

        return ($"\nCommand completed with exit code: {result.ExitCode}\n", color);
    }

    public (string FormattedText, SolidColorBrush Color) FormatError(string errorMessage)
    {
        return ($"Error executing command: {errorMessage}\n", new SolidColorBrush(Colors.Red));
    }
}