namespace JetBrainsInterviewProject.Interfaces;

public interface IDispatcherService
{
    void InvokeOnUIThread(Action action);
}