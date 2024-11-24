using System.Reflection;
using System;
using System.Runtime.CompilerServices;
using AudioReplacer2.Util;
using Microsoft.UI.Xaml;

namespace AudioReplacer2
{
    public partial class App : Application
    {
        public static MainWindow MainWindow { get; private set; }
        public static ApplicationTheme systemAppTheme { get; private set; } = ApplicationTheme.Dark;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }

    }
}
