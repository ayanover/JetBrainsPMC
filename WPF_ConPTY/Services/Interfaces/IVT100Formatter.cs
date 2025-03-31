namespace WPF_ConPTY.Services.Interfaces
{
    /// <summary>
    /// Interface for VT100 text formatter
    /// </summary>
    public interface IVT100Formatter
    {
        /// <summary>
        /// Process text with VT100 sequences
        /// </summary>
        void ProcessText(string text);
    }
}