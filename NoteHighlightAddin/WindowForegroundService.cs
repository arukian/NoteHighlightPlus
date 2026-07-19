using System;
using System.Runtime.InteropServices;

namespace NoteHighlightAddin
{
    public class WindowForegroundService
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(
            IntPtr hWnd);

        public bool BringToFront(
            IntPtr windowHandle)
        {
            return SetForegroundWindow(
                windowHandle);
        }
    }
}