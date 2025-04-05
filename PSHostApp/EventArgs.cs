namespace PSHostApp;

public class OutputReceivedEventArgs : EventArgs
{
    public string Output { get; }
        
    public OutputReceivedEventArgs(string output)
    {
        Output = output;
    }
}