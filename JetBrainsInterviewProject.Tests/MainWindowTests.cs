using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using JetBrainsInterviewProject.Services;

namespace JetBrainsInterviewProject.Tests
{
    public class MainWindowTests
    {
        private readonly Mock<ICommandExecutionService> _mockCommandService;
        private readonly MainWindow _window;

        public MainWindowTests()
        {
            _mockCommandService = new Mock<ICommandExecutionService>();
            _window = new MainWindow(_mockCommandService.Object);
        }

        [Fact]
        public async Task ExecuteCommand_CallsService()
        {
            // Arrange
            _mockCommandService.Setup(m => m.ExecuteCommandAsync(It.IsAny<string>()))
                .ReturnsAsync(new CommandResult { ExitCode = 0 });

            // We need to access the private method via reflection
            var method = typeof(MainWindow).GetMethod("ExecuteCommand", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Set the text in the input box
            _window.CommandInputTextBox.Text = "test command";

            // Act
            var task = (Task)method.Invoke(_window, null);
            await task;

            // Assert
            _mockCommandService.Verify(m => m.ExecuteCommandAsync("test command"), Times.Once);
        }

        [Fact]
        public async Task ExecuteCommand_EmptyCommand_DoesNotCallService()
        {
            // Arrange
            _window.CommandInputTextBox.Text = "";
            
            var method = typeof(MainWindow).GetMethod("ExecuteCommand", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var task = (Task)method.Invoke(_window, null);
            await task;

            // Assert
            _mockCommandService.Verify(m => m.ExecuteCommandAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteCommand_SuccessResult_UpdatesStatusText()
        {
            // Arrange
            _mockCommandService.Setup(m => m.ExecuteCommandAsync(It.IsAny<string>()))
                .ReturnsAsync(new CommandResult { ExitCode = 0 });
            
            _window.CommandInputTextBox.Text = "test command";
            
            var method = typeof(MainWindow).GetMethod("ExecuteCommand", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var task = (Task)method.Invoke(_window, null);
            await task;

            // Assert
            Assert.Equal("Command executed successfully", _window.StatusText.Text);
        }

        [Fact]
        public async Task ExecuteCommand_ErrorResult_UpdatesStatusTextWithExitCode()
        {
            // Arrange
            _mockCommandService.Setup(m => m.ExecuteCommandAsync(It.IsAny<string>()))
                .ReturnsAsync(new CommandResult { ExitCode = 1 });
            
            _window.CommandInputTextBox.Text = "test command";
            
            var method = typeof(MainWindow).GetMethod("ExecuteCommand", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var task = (Task)method.Invoke(_window, null);
            await task;

            // Assert
            Assert.Equal("Command exited with code 1", _window.StatusText.Text);
        }

        [Fact]
        public async Task ExecuteCommand_Exception_DisplaysErrorMessage()
        {
            // Arrange
            _mockCommandService.Setup(m => m.ExecuteCommandAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));
            
            _window.CommandInputTextBox.Text = "test command";
            
            var method = typeof(MainWindow).GetMethod("ExecuteCommand", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var task = (Task)method.Invoke(_window, null);
            await task;

            // Assert
            Assert.Contains("Test error", _window.CommandOutput.Text);
            Assert.Equal("Command execution failed", _window.StatusText.Text);
        }
    }
}