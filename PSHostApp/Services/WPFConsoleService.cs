using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using PSHostApp.Interfaces;
using PSHostApp.Enum;

namespace PSHostApp.Services
{
    public class WpfConsoleService : IConsoleService
    {
        private readonly RichTextBox _outputTextBox;

        public WpfConsoleService(RichTextBox outputTextBox)
        {
            _outputTextBox = outputTextBox;
        }

        public void AppendToConsole(string text, ConsoleMessageType messageType)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => AppendToConsole(text, messageType));
                return;
            }

            Color textColor = GetColorForMessageType(messageType);

            TextRange textRange = new TextRange(_outputTextBox.Document.ContentEnd, _outputTextBox.Document.ContentEnd);
            textRange.Text = text;
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(textColor));
            
            _outputTextBox.ScrollToEnd();
        }

        public void Clear()
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(Clear);
                return;
            }

            _outputTextBox.Document.Blocks.Clear();
        }

        private Color GetColorForMessageType(ConsoleMessageType messageType)
        {
            return messageType switch
            {
                ConsoleMessageType.Success => Colors.LightGreen,
                ConsoleMessageType.Error => Colors.Red,
                ConsoleMessageType.Warning => Colors.Yellow,
                ConsoleMessageType.Info => Colors.LightBlue,
                ConsoleMessageType.Prompt => Colors.Cyan,
                _ => Colors.White
            };
        }
    }
}