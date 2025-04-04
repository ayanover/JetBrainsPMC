using PSHostApp.Interfaces;
using PSHostApp.Enum;

namespace PSHostApp.Interfaces
{
    public interface IConsoleService
    {
        void AppendToConsole(string text, ConsoleMessageType messageType);
        void Clear();
    }
}