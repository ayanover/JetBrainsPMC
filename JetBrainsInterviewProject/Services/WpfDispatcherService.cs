using System.Windows;
using JetBrainsInterviewProject.Interfaces;

namespace JetBrainsInterviewProject.Services;

public class WpfDispatcherService : IDispatcherService
{
    public void InvokeOnUIThread(Action action)
    {
        Application.Current.Dispatcher.Invoke(action);
    }
}