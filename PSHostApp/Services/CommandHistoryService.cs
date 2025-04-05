using System.Collections.Generic;

namespace PSHostApp.Services
{
    public interface ICommandHistoryService
    {
        void AddCommand(string command);
        string NavigateUp();
        string NavigateDown();
        void ResetNavigation();
    }
    public class CommandHistoryService : ICommandHistoryService
    {
        private readonly List<string> _commandHistory = new List<string>();
        private int _historyIndex = -1;

        public void AddCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            _commandHistory.Add(command);
            _historyIndex = _commandHistory.Count;
        }

        public string NavigateUp()
        {
            if (_commandHistory.Count == 0 || _historyIndex <= 0)
                return string.Empty;

            _historyIndex--;
            return _commandHistory[_historyIndex];
        }

        public string NavigateDown()
        {
            if (_commandHistory.Count == 0 || _historyIndex >= _commandHistory.Count - 1)
            {
                _historyIndex = _commandHistory.Count;
                return string.Empty;
            }

            _historyIndex++;
            return _commandHistory[_historyIndex];
        }

        public void ResetNavigation()
        {
            _historyIndex = _commandHistory.Count;
        }
    }
}