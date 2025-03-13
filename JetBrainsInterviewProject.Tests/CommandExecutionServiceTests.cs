using System;
using System.Threading.Tasks;
using Xunit;
using JetBrainsInterviewProject.Services;

namespace JetBrainsInterviewProject.Tests
{
    public class CommandExecutionServiceTests
    {
        private readonly CommandExecutionService _service;

        public CommandExecutionServiceTests()
        {
            _service = new CommandExecutionService();
        }

        [Fact]
        public async Task ExecuteCommandAsync_EmptyCommand_ThrowsArgumentException()
        {
            string emptyCommand = string.Empty;
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExecuteCommandAsync(emptyCommand));
        }

        [Fact]
        public async Task ExecuteCommandAsync_EchoCommand_ReturnsExpectedOutput()
        {
            string command = "echo Test Output";
            string expected = "Test Output";
            
            var result = await _service.ExecuteCommandAsync(command);
            
            Assert.Contains(expected, result.Output);
            Assert.Equal(0, result.ExitCode);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ExecuteCommandAsync_Directory_ReturnsSuccessResult()
        {
            // Arrange
            string command = "dir";

            // Act
            var result = await _service.ExecuteCommandAsync(command);

            // Assert
            Assert.NotEmpty(result.Output);
            Assert.Equal(0, result.ExitCode);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ExecuteCommandAsync_InvalidCommand_ReturnsErrorCode()
        {
            // Arrange
            string command = "thiscommanddoesnotexist";

            // Act
            var result = await _service.ExecuteCommandAsync(command);

            // Assert
            Assert.Contains("is not recognized", result.Output);
            Assert.NotEqual(0, result.ExitCode);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task ExecuteCommandAsync_OutputEvent_FiresWithData()
        {
            // Arrange
            string command = "echo Test Event";
            string receivedOutput = null;
            _service.OutputReceived += (data) => receivedOutput = data;

            // Act
            await _service.ExecuteCommandAsync(command);

            // Assert
            Assert.Equal("Test Event", receivedOutput);
        }
    }
}