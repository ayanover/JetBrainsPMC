using System.Threading.Tasks;

namespace WPF_ConPTY.Services.Interfaces
{
    /// <summary>
    /// Interface for command interceptor
    /// </summary>
    public interface ICommandInterceptor
    {
        /// <summary>
        /// Register a command handler
        /// </summary>
        void RegisterHandler(ICommandHandler handler);

        /// <summary>
        /// Try to handle a command
        /// </summary>
        Task<CommandResult> TryHandleCommandAsync(string command);
    }

    /// <summary>
    /// Interface for command handlers
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// The prefix that identifies commands this handler can process
        /// </summary>
        string CommandPrefix { get; }

        /// <summary>
        /// Handles a command
        /// </summary>
        Task<CommandResult> HandleCommandAsync(string command);
    }

    /// <summary>
    /// Specific interface for NuGet command handler
    /// </summary>
    public interface INuGetCommandHandler : ICommandHandler
    {
    }

    /// <summary>
    /// Result of a command handling operation
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// Whether the command was handled
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Output to display to the user
        /// </summary>
        public string Output { get; set; }
    }
}