using System;
using System.Threading.Tasks;
using PSHostApp.Enum;

namespace PSHostApp.Interfaces
{
    public interface IPowerShellService : IDisposable
    {
        Task<string> ExecuteCommandAsync(string command);
        void Initialize();
        event EventHandler<ConsoleMessageEventArgs> MessageReceived;
    }
    public class ConsoleMessageEventArgs : EventArgs
    {
        public string Message { get; }
        public ConsoleMessageType MessageType { get; }

        public ConsoleMessageEventArgs(string message, ConsoleMessageType messageType)
        {
            Message = message;
            MessageType = messageType;
        }
    }
    
}