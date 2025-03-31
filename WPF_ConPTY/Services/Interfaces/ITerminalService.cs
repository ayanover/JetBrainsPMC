
namespace WPF_ConPTY.Services.Interfaces
{
    /// <summary>
    /// Interface for terminal service
    /// </summary>
    public interface ITerminalService
    {
        /// <summary>
        /// Start the terminal with a command
        /// </summary>
        Task StartTerminalAsync(string command, int width = 120, int height = 30);

        /// <summary>
        /// Send a command to the terminal
        /// </summary>
        void SendCommand(string command, bool displayInOutput = true);

        /// <summary>
        /// Close the terminal
        /// </summary>
        void CloseTerminal();
    }
}