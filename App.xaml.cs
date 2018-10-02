using System;
using System.IO;
using System.Windows;

namespace HotkeyMusicPlayer
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                string directory = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                string path = Path.Combine(directory, "HotkeyMusicExceptionLog.txt");

                File.WriteAllText(path, ToString(e.Exception));
            }
            catch { }
        }

        private string ToString(Exception e)
        {
            string stackTrace = e.StackTrace;
            string message = string.Empty;

            while (e != null)
            {
                message += e.GetType() + ":\r\n";
                message += e.Message + "\r\n\r\n";

                e = e.InnerException;
            }

            message += stackTrace;
            return message;
        }
    }
}
