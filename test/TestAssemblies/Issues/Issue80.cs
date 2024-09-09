using Issues.Attributes;
using System;
using System.Runtime.InteropServices;

namespace Issues
{
    [_80_]
    public class Issue80
    {
        public void M() { }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
