using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using static ConPTY.Native.PseudoConsoleApi;

namespace ConPTY
{
    /// <summary>
    /// Utility functions around the new Pseudo Console APIs.
    /// </summary>
    internal sealed class PseudoConsole : IDisposable
    {
        public static readonly IntPtr PseudoConsoleThreadAttribute = (IntPtr)PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE;

        public IntPtr Handle { get; }

        private PseudoConsole(IntPtr handle)
        {
            this.Handle = handle;
        }

        internal static PseudoConsole Create(SafeFileHandle inputReadSide, SafeFileHandle outputWriteSide, int width, int height)
        {
            var createResult = CreatePseudoConsole(
                new COORD { X = (short)width, Y = (short)height },
                inputReadSide, outputWriteSide,
                0, out IntPtr hPC);
            if(createResult != 0)
            {                             
                throw new Win32Exception(createResult, "Could not create pseudo console.");
            }
            return new PseudoConsole(hPC);
        }

        public void Dispose()
        {
            ClosePseudoConsole(Handle);
        }
    }
}
