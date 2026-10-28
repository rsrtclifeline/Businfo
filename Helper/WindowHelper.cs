using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Businfo
{
    public static class WindowHelper
    {
        public static WindowCollection windows
        {
            get => Application.Current.Windows;
        }
        public static T ShowSingleInstance<T>() where T : Window, new()
        {
            var win = Application.Current.Windows.OfType<T>().FirstOrDefault();
            if (win == null)
            {
                win = new T();
                win.Show();
            }
            else
            {
                if (win.WindowState == WindowState.Minimized)
                {
                    win.WindowState = WindowState.Normal;
                }
                win.Activate();
            }
            return win;
        }

        public static T GetWindow<T>() where T : Window
        {
            return windows.OfType<T>().FirstOrDefault();
        }

        public static void MinimizeAll()
        {
            foreach (Window w in windows)
            {
                w.Minimize();
            }
        }

        public static void CloseAllExceptMainWindow()
        {
            foreach(Window w in windows)
            {
                if (!(w is MainWindow))
                    w.Close();
            }
        }

        public static void CloseAllSubWExcept<T>() where T : Window
        {
            foreach (Window w in windows)
            {
                if (!(w is T) && !(w is MainWindow)) 
                    w.Close();
            }
        }

        public static void Minimize(this Window window)
        {
            window.WindowState = WindowState.Minimized;
        }
    }
}
