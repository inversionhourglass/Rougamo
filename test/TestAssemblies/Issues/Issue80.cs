using Issues.Attributes;
using System;
using System.Runtime.InteropServices;

namespace Issues
{
    [_80_]
    public partial class Issue80
    {
        public void M() { }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

#if NET7_0_OR_GREATER
        [LibraryImport("libSystem.dylib")]
        private static partial int getpid();
#endif
    }
}
