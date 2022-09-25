using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows;
using Snippets;

namespace WpfApp_AutoPlay
{
    internal class MouseController
    {
        //use external method from user32.dll
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        public static MouseController instance;

        private Record record;

        public MouseController()
        {
            instance = this;
        }

        public void SetMousePosition(int xPos, int yPos)
        {
            Trace.WriteLine("Testing");
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(xPos, yPos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0);
        }
    }
}