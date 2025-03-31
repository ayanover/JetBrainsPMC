using System;

namespace ConPTY
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create and configure your terminal
            var terminal = new ConPTY.Terminal();
            
            terminal.OutputReady += (sender, e) =>
            {
                // Handle terminal output ready
                Console.WriteLine("Terminal ready");
            };
            
            // Start the terminal with cmd.exe or another command
            terminal.Start("cmd.exe");
        }
    }
}