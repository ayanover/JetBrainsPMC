
namespace WPF_ConPTY.Services.Interfaces
{
    public interface ITerminalService
    {
        Task StartTerminalAsync(string command, int width = 80, int height = 120);
        void SendCommand(string command, bool displayInOutput = true);
        void CloseTerminal();
    }
}