using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HotkeyMusicPlayer
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public static string[] StartupArgs { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            StartupArgs = e.Args;   

            base.OnStartup(e);
        }
    }
}
