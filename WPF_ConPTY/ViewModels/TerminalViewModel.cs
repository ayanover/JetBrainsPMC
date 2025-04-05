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
    public class TerminalViewModel : INotifyPropertyChanged
    {
        private readonly ITerminalService _terminalService;
        private string _commandInput;
        public string CommandInput
        {
            get => _commandInput;
            set
            {
                if (_commandInput != value)
                {
                    _commandInput = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested(); 
                }
            }
        }

        public ICommand SendCommand { get; }

        public ICommand ClearCommand { get; }

        public TerminalViewModel(ITerminalService terminalService)
        {
            _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));

            SendCommand = new RelayCommand(ExecuteSendCommand, CanExecuteSendCommand);
            ClearCommand = new RelayCommand(ExecuteClearCommand);
        }

        public async Task InitializeTerminalAsync()
        {
            await _terminalService.StartTerminalAsync(
                "powershell.exe -NoProfile -NoExit -Command \"function prompt { return \\\"`n>> \\\" }; " +
                "$Host.UI.RawUI.BufferSize = New-Object System.Management.Automation.Host.Size(120, 120); " +
                "$Host.UI.RawUI.WindowSize = New-Object System.Management.Automation.Host.Size(120, 120); " +
                "Set-PSReadLineOption -HistorySaveStyle SaveNothing -ShowToolTips:$false -AddToHistoryHandler { return $false }\"",
                120, 120);
        }

            
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