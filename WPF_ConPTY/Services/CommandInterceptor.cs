using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WPF_ConPTY.Services.Interfaces;

namespace WPF_ConPTY.Services
{
    /// <summary>
    /// Intercepts and processes custom commands before they are sent to the terminal
    /// </summary>
    public class CommandInterceptor : ICommandInterceptor
    {
        private readonly Dictionary<string, ICommandHandler> _commandHandlers = new Dictionary<string, ICommandHandler>();
        private readonly IServiceProvider _serviceProvider;

        public CommandInterceptor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            RegisterBuiltInHandlers();
        }

        private void RegisterBuiltInHandlers()
        {
            var nuGetHandler = _serviceProvider.GetService<INuGetCommandHandler>();
            if (nuGetHandler != null)
            {
                RegisterHandler(nuGetHandler);
            }

        }

        /// <summary>
        /// Registers a command handler
        /// </summary>
        public void RegisterHandler(ICommandHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _commandHandlers[handler.CommandPrefix] = handler;
        }

        /// <summary>
        /// Attempts to intercept and handle a command
        /// </summary>
        /// <returns>True if handled, false if not</returns>
        public async Task<CommandResult> TryHandleCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return new CommandResult { Handled = false };

            foreach (var handler in _commandHandlers.Values)
            {
                if (command.StartsWith(handler.CommandPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return await handler.HandleCommandAsync(command);
                }
            }

            return new CommandResult { Handled = false };
        }
    }
}