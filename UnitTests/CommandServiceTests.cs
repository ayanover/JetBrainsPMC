using JetBrainsInterviewProject.Services;

namespace JetBrainsInterviewProject.UnitTests
{
    public class CommandExecutionServiceTests
    {
        private readonly CommandExecutionService _service;

        public CommandExecutionServiceTests()
        {
            _service = new CommandExecutionService();
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldThrowArgumentException_WhenCommandIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExecuteCommandAsync(string.Empty));
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldExecuteSuccessfulCommand_AndReturnResultWithZeroExitCode()
        {
            string command = "echo 'test'";
            bool outputEventFired = false;
            
            _service.OutputReceived += (data, type) => {
                outputEventFired = true;
            };

            var result = await _service.ExecuteCommandAsync(command);

            Assert.NotNull(result);
            Assert.Equal(0, result.ExitCode);
            Assert.True(result.IsSuccess);
            Assert.True(outputEventFired);
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldExecuteFailingCommand_AndReturnNonZeroExitCode()
        {
            string command = "throw 'error'";

            var result = await _service.ExecuteCommandAsync(command);
            
            Assert.NotNull(result);
            Assert.NotEqual(0, result.ExitCode);
            Assert.False(result.IsSuccess);
        }
    }
}