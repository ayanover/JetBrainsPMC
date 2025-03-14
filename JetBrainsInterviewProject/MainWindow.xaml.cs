using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace JetBrainsInterviewProject
{
    public partial class MainWindow : Window
    {
        private readonly CommandWindowViewModel _viewModel;

        public MainWindow(CommandWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
            InitializeComponent();
            
            _viewModel.AppendTextRequested += OnAppendTextRequested;
        }

        private void CommandInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _viewModel.ExecuteCommand.CanExecute(null))
            {
                _viewModel.ExecuteCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void OnAppendTextRequested(string text, SolidColorBrush color)
        {
            if (string.IsNullOrEmpty(text))
            {
                CommandOutput.Document.Blocks.Clear();
                return;
            }
            
            TextRange tr = new TextRange(CommandOutput.Document.ContentEnd, CommandOutput.Document.ContentEnd);
            tr.Text = text;
            
            if (color != null)
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            }
            
            CommandOutput.ScrollToEnd();
        }
    }
}