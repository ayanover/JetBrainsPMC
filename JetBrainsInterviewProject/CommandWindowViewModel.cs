using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using JetBrainsInterviewProject.Enums;
using JetBrainsInterviewProject.Interfaces;

public class CommandWindowViewModel : INotifyPropertyChanged
{
    private readonly ICommandExecutionService _commandService;
    private readonly IOutputFormatter _outputFormatter;
    private readonly IDispatcherService _dispatcherService;
    private string _commandText = string.Empty;
    private string _statusText = "Ready";
    private bool _isExecuting;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public string CommandText 
    { 
        get => _commandText; 
        set 
        { 
            _commandText = value; 
            OnPropertyChanged();
            (ExecuteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        } 
    }

    public string StatusText 
    { 
        get => _statusText; 
        set { _statusText = value; OnPropertyChanged(); } 
    }

    public bool IsExecuting 
    { 
        get => _isExecuting;
        set 
        { 
            _isExecuting = value; 
            OnPropertyChanged();
            (ExecuteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public ICommand ExecuteCommand { get; }

    public event Action<string, SolidColorBrush>? AppendTextRequested;
    
    public CommandWindowViewModel(
        ICommandExecutionService commandService,
        IOutputFormatter outputFormatter,
        IDispatcherService dispatcherService)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _outputFormatter = outputFormatter ?? throw new ArgumentNullException(nameof(outputFormatter));
        _dispatcherService = dispatcherService ?? throw new ArgumentNullException(nameof(dispatcherService));
        
        _commandService.OutputReceived += OnOutputReceived;
        
        ExecuteCommand = new RelayCommand(
            async () => await ExecuteCommandAsync(),
            () => !string.IsNullOrWhiteSpace(CommandText) && !IsExecuting
        );
        
        StatusText = "Ready to execute commands. Type a command and press Enter or click Execute.";
    }

    private void OnOutputReceived(string data, OutputType type)
    {
        _dispatcherService.InvokeOnUIThread(() =>
        {
            var (formattedText, color) = _outputFormatter.FormatOutput(data, type);
            AppendTextRequested?.Invoke(formattedText, color);
        });
    }

    private async Task ExecuteCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(CommandText))
            return;

        try
        {
            IsExecuting = true;
            StatusText = "Executing command...";
            
            _dispatcherService.InvokeOnUIThread(() =>
            {
                AppendTextRequested?.Invoke(string.Empty, null);
            });
            
            _dispatcherService.InvokeOnUIThread(() =>
            {
                var (formattedText, color) = _outputFormatter.FormatCommandExecutionStart(CommandText);
                AppendTextRequested?.Invoke(formattedText, color);
            });
            
            var result = await _commandService.ExecuteCommandAsync(CommandText);
            
            StatusText = result.IsSuccess 
                ? "Command executed successfully" 
                : $"Command exited with code {result.ExitCode}";

            _dispatcherService.InvokeOnUIThread(() =>
            {
                var (formattedText, color) = _outputFormatter.FormatCommandResult(result);
                AppendTextRequested?.Invoke(formattedText, color);
            });
        }
        catch (Exception ex)
        {
            StatusText = "Command execution failed";
            _dispatcherService.InvokeOnUIThread(() =>
            {
                var (formattedText, color) = _outputFormatter.FormatError(ex.Message);
                AppendTextRequested?.Invoke(formattedText, color);
            });
        }
        finally
        {
            IsExecuting = false;
        }
    }
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
    
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (() => true);
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute();

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
