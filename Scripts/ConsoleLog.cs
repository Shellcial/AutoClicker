using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp_AutoPlay
{
    internal class ConsoleLog
    {
        MainWindow _MainWindow = Application.Current.Windows[0] as MainWindow;

        public static ConsoleLog instance;

        public ConsoleLog()
        {
            instance = this; 
        }


        public void Log(string _content)
        {
            if (_MainWindow.debugBox.Text == "")
            {
                _MainWindow.debugBox.Text = _content;
            }
            else
            {
                _MainWindow.debugBox.Text = _MainWindow.debugBox.Text + Environment.NewLine + _content;
            }

        }

    }
}
