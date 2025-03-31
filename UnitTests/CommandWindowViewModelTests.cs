using System.Windows.Media;
using JetBrainsInterviewProject.DTO;
using JetBrainsInterviewProject.Interfaces;
using Moq;

namespace JetBrainsInterviewProject.UnitTests
{
    public class CommandWindowViewModelTests
    {
        private readonly Mock<ICommandExecutionService> _mockCommandService;
        private readonly Mock<IOutputFormatter> _mockOutputFormatter;
        private readonly Mock<IDispatcherService> _mockDispatcherService;
        private readonly CommandWindowViewModel _viewModel;

        public CommandWindowViewModelTests()
        {
            _mockCommandService = new Mock<ICommandExecutionService>();
            _mockOutputFormatter = new Mock<IOutputFormatter>();
            _mockDispatcherService = new Mock<IDispatcherService>();
            
            _mockDispatcherService.Setup(d => d.InvokeOnUIThread(It.IsAny<Action>()))
                .Callback<Action>(action => action());
                
            _viewModel = new CommandWindowViewModel(
                _mockCommandService.Object,
                _mockOutputFormatter.Object,
                _mockDispatcherService.Object);
        }

        [Fact]
        public void ExecuteCommand_ShouldBeExecutable_OnlyWhenCommandTextIsNotEmptyAndNotExecuting()
        {
            _viewModel.CommandText = string.Empty;
            _viewModel.IsExecuting = false;
            Assert.False(((RelayCommand)_viewModel.ExecuteCommand).CanExecute(null));
            
            _viewModel.CommandText = "valid command";
            _viewModel.IsExecuting = true;
            Assert.False(((RelayCommand)_viewModel.ExecuteCommand).CanExecute(null));
            
            _viewModel.CommandText = "valid command";
            _viewModel.IsExecuting = false;
            Assert.True(((RelayCommand)_viewModel.ExecuteCommand).CanExecute(null));
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldUpdateStatusAndUI_WhenCommandSucceeds()
        {
            _viewModel.CommandText = "test command";
            var successResult = new CommandResult { ExitCode = 0 };
            
            _mockCommandService.Setup(s => s.ExecuteCommandAsync("test command"))
                .ReturnsAsync(successResult);
                
            _mockOutputFormatter.Setup(f => f.FormatCommandExecutionStart(It.IsAny<string>()))
                .Returns(("command start", new SolidColorBrush(Colors.Blue)));
                
            _mockOutputFormatter.Setup(f => f.FormatCommandResult(It.IsAny<CommandResult>()))
                .Returns(("success result", new SolidColorBrush(Colors.Green)));
            
            bool appendTextWasCalled = false;
            _viewModel.AppendTextRequested += (text, color) => {
                if (!string.IsNullOrEmpty(text)) {
                    appendTextWasCalled = true;
                }
            };
            
            await ExecuteRelayCommandAsync(_viewModel.ExecuteCommand as RelayCommand);
            
            Assert.Equal("Command executed successfully", _viewModel.StatusText);
            Assert.False(_viewModel.IsExecuting);
            Assert.True(appendTextWasCalled);
            _mockCommandService.Verify(s => s.ExecuteCommandAsync("test command"), Times.Once);
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldHandleException_AndUpdateUI()
        {
            _viewModel.CommandText = "fail command";
            
            _mockCommandService.Setup(s => s.ExecuteCommandAsync("fail command"))
                .ThrowsAsync(new Exception("Command failed"));
                
            _mockOutputFormatter.Setup(f => f.FormatCommandExecutionStart(It.IsAny<string>()))
                .Returns(("command start", new SolidColorBrush(Colors.Blue)));
                
            _mockOutputFormatter.Setup(f => f.FormatError(It.IsAny<string>()))
                .Returns(("error message", new SolidColorBrush(Colors.Red)));
            
            await ExecuteRelayCommandAsync(_viewModel.ExecuteCommand as RelayCommand);
            
            Assert.Equal("Command execution failed", _viewModel.StatusText);
            Assert.False(_viewModel.IsExecuting);
            _mockOutputFormatter.Verify(f => f.FormatError(It.IsAny<string>()), Times.Once);
        }
        
        private async Task ExecuteRelayCommandAsync(RelayCommand command)
        {
            command.Execute(null);
            await Task.Delay(50); 
        }
    }
}