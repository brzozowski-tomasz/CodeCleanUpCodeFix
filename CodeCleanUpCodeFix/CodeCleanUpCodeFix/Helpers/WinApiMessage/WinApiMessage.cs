using System;
using System.Runtime.InteropServices;
using CodeCleanUpCodeFix.Helpers.WinApiMessage.Interfaces;

namespace CodeCleanUpCodeFix.Helpers.WinApiMessage
{
    public class WinApiMessageBox: IWinApiMessageBox
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

        public void Show(string caption, string message)
        {
            MessageBox(new IntPtr(0), message, caption, 0);
        }
    }
}
