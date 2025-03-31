namespace ConPTY
{
    class Program
    {
        //Unused, it's here because my IDE kept complaining about it
        static void Main(string[] args)
        {
            var terminal = new ConPTY.Terminal();
            
            terminal.OutputReady += (sender, e) =>
            {
                Console.WriteLine("Terminal ready");
            };
            
            terminal.Start("cmd.exe");
        }
    }
}