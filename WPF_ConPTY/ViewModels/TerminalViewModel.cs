using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using WPF_ConPTY.Services.Interfaces;
using WPF_ConPTY.Commands;
using WPF_ConPTY.Services;

namespace WPF_ConPTY.ViewModels
{
    /// <summary>
    /// ViewModel for the terminal UI
    /// </summary>
    public class TerminalViewModel : INotifyPropertyChanged
    {
        private readonly ITerminalService _terminalService;
        private string _commandInput;

        /// <summary>
        /// The current command input
        /// </summary>
        public string CommandInput
        {
            get => _commandInput;
            set
            {
                if (_commandInput != value)
                {
                    _commandInput = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested(); // Refresh command can-execute state
                }
            }
        }

        /// <summary>
        /// Command to send the current input to the terminal
        /// </summary>
        public ICommand SendCommand { get; }

        /// <summary>
        /// Command to clear the terminal
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Creates a new terminal view model
        /// </summary>
        /// <param name="terminalService">The terminal service</param>
        public TerminalViewModel(ITerminalService terminalService)
        {
            _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));

            SendCommand = new RelayCommand(ExecuteSendCommand, CanExecuteSendCommand);
            ClearCommand = new RelayCommand(ExecuteClearCommand);
        }

        /// <summary>
        /// Start the terminal
        /// </summary>
        public async Task InitializeTerminalAsync()
        {
            await _terminalService.StartTerminalAsync(
                "powershell.exe -NoProfile -NoExit -Command \"function prompt { return '>> ' }; " +
                "Set-PSReadLineOption -HistorySaveStyle SaveNothing -ShowToolTips:$false -AddToHistoryHandler { return $false }\"",
                120, 30);
        }

        /// <summary>
        /// Show a welcome message or logo
        /// </summary>
        

        private bool CanExecuteSendCommand(object parameter)
        {
            return !string.IsNullOrWhiteSpace(CommandInput);
        }

        private void ExecuteSendCommand(object parameter)
        {
            string command = CommandInput?.Trim();
            if (!string.IsNullOrEmpty(command))
            {
                _terminalService.SendCommand(command);
                CommandInput = string.Empty;
            }
        }

        private void ExecuteClearCommand(object parameter)
        {
            _terminalService.SendCommand("cls", false);
        }

        /// <summary>
        /// Close the terminal when the application is shutting down
        /// </summary>
        public void Shutdown()
        {
            _terminalService.CloseTerminal();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}