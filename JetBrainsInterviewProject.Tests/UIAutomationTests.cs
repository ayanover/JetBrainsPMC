using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Reactive;
using Xunit;

namespace ShellCommandExecutor.Tests
{
    public class UIAutomationTests : IDisposable
    {
        private System.Diagnostics.Process _appProcess;
        private AutomationElement _appWindow;

        public UIAutomationTests()
        {
            // Start the application
            _appProcess = System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ShellCommandExecutor.exe",
                    UseShellExecute = true
                });

            // Wait for the application to start
            Thread.Sleep(1000);

            // Find the main window
            _appWindow = AutomationElement.FromHandle(_appProcess.MainWindowHandle);
        }

        [Fact(Skip = "This is an integration test that requires the app to be built")]
        public void ApplicationLaunches_MainWindowIsDisplayed()
        {
            // Assert
            Assert.NotNull(_appWindow);
            Assert.Equal("Shell Command Executor", _appWindow.Current.Name);
        }

        [Fact(Skip = "This is an integration test that requires the app to be built")]
        public void ExecuteButton_ClickExecutesCommand()
        {
            // Arrange
            var inputTextBox = _appWindow.FindFirst(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "CommandInputTextBox"));

            var executeButton = _appWindow.FindFirst(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "ExecuteButton"));

            var outputTextBox = _appWindow.FindFirst(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "CommandOutput"));

            // Act
            // Set text in the input textbox
            ValuePattern inputValuePattern = (ValuePattern)inputTextBox.GetCurrentPattern(ValuePattern.Pattern);
            inputValuePattern.SetValue("echo UI Automation Test");

            // Click the Execute button
            InvokePattern executeButtonPattern = (InvokePattern)executeButton.GetCurrentPattern(InvokePattern.Pattern);
            executeButtonPattern.Invoke();

            // Wait for command execution
            Thread.Sleep(1000);

            // Get the output text
            ValuePattern outputValuePattern = (ValuePattern)outputTextBox.GetCurrentPattern(ValuePattern.Pattern);
            string outputText = outputValuePattern.Current.Value;

            // Assert
            Assert.Contains("UI Automation Test", outputText);
        }

        [Fact(Skip = "This is an integration test that requires the app to be built")]
        public void EnterKey_ExecutesCommand()
        {
            // Arrange
            var inputTextBox = _appWindow.FindFirst(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "CommandInputTextBox"));

            var outputTextBox = _appWindow.FindFirst(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "CommandOutput"));

            // Act
            // Set text in the input textbox
            ValuePattern inputValuePattern = (ValuePattern)inputTextBox.GetCurrentPattern(ValuePattern.Pattern);
            inputValuePattern.SetValue("echo Enter Key Test");

            // Send Enter key
            inputTextBox.SetFocus();
            System.Windows.Forms.SendKeys.SendWait("{ENTER}");

            // Wait for command execution
            Thread.Sleep(1000);

            // Get the output text
            ValuePattern outputValuePattern = (ValuePattern)outputTextBox.GetCurrentPattern(ValuePattern.Pattern);
            string outputText = outputValuePattern.Current.Value;

            // Assert
            Assert.Contains("Enter Key Test", outputText);
        }

        public void Dispose()
        {
            if (_appProcess != null && !_appProcess.HasExited)
            {
                _appProcess.Kill();
                _appProcess.Dispose();
            }
        }
    }
}