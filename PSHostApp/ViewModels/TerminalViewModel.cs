using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using PSHostApp.Interfaces;
using PSHostApp.Services;
using PSHostApp.Enum;

namespace PSHostApp.ViewModels
{
    public class TerminalViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IPowerShellService _powerShellService;
        private readonly IConsoleService _consoleService;
        private readonly ICommandHistoryService _historyService;
        private string _commandInput;
        private bool _disposed = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CommandInput
        {
            get => _commandInput;
            set
            {
                _commandInput = value;
                OnPropertyChanged();
            }
        }

        public ICommand SendCommand { get; }
        public ICommand NavigateHistoryUpCommand { get; }
        public ICommand NavigateHistoryDownCommand { get; }
        public ICommand ClearCommand { get; }

        public TerminalViewModel(
            IPowerShellService powerShellService,
            IConsoleService consoleService,
            ICommandHistoryService historyService)
        {
            _powerShellService = powerShellService ?? throw new ArgumentNullException(nameof(powerShellService));
            _consoleService = consoleService ?? throw new ArgumentNullException(nameof(consoleService));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));

            SendCommand = new RelayCommand(_ => ExecuteCommandAsync(), _ => !string.IsNullOrWhiteSpace(CommandInput));
            NavigateHistoryUpCommand = new RelayCommand(_ => NavigateHistoryUp());
            NavigateHistoryDownCommand = new RelayCommand(_ => NavigateHistoryDown());
            ClearCommand = new RelayCommand(_ => _consoleService.Clear());

            _powerShellService.MessageReceived += OnPowerShellMessageReceived;
            
            _powerShellService.Initialize();
        }

        private void OnPowerShellMessageReceived(object sender, ConsoleMessageEventArgs e)
        {
            _consoleService.AppendToConsole(e.Message, e.MessageType);
        }

        private async void ExecuteCommandAsync()
        {
            if (string.IsNullOrWhiteSpace(CommandInput))
                return;

            string command = CommandInput;
            
            _historyService.AddCommand(command);
            
            _consoleService.AppendToConsole(command + "\n", ConsoleMessageType.Normal);
            
            CommandInput = string.Empty;
            
            string result = await _powerShellService.ExecuteCommandAsync(command);
            
            if (!string.IsNullOrEmpty(result))
            {
                _consoleService.AppendToConsole(result, ConsoleMessageType.Normal);
            }
            
            _consoleService.AppendToConsole("\nPS> ", ConsoleMessageType.Prompt);
        }

        private void NavigateHistoryUp()
        {
            string command = _historyService.NavigateUp();
            if (!string.IsNullOrEmpty(command))
            {
                CommandInput = command;
            }
        }

        private void NavigateHistoryDown()
        {
            string command = _historyService.NavigateDown();
            CommandInput = command; // Can be empty
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _powerShellService.MessageReceived -= OnPowerShellMessageReceived;
                    
                    _powerShellService.Dispose();
                }

                _disposed = true;
            }
        }

        ~TerminalViewModel()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Simple ICommand implementation
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}