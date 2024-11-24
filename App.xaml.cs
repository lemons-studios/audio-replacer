using System;
using System.IO;
using System.Text.Json;
using AudioReplacer2.Util;
using Config.Net;
using Microsoft.UI.Xaml;

namespace AudioReplacer2
{
    public partial class App : Application
    {
        public static IAppSettings AppSettings { get; private set; }

        public static MainWindow MainWindow { get; private set; }
        public static ApplicationTheme SystemAppTheme { get; set; } = ApplicationTheme.Dark;
        private readonly string directoryPath, filePath;

        public App()
        {
            directoryPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Config";
            filePath = $"{directoryPath}\\AudioReplacer2-Config.json";
            CreateSettingsData();

            AppSettings = new ConfigurationBuilder<IAppSettings>()
                .UseJsonFile(filePath)
                .Build();

            GlobalData.updateChecksAllowed = AppSettings.AppUpdateCheck == 1;
            GlobalData.notificationTimeout = AppSettings.NotificationTimeout;
            GlobalData.recordStopDelay = AppSettings.RecordEndWaitTime;

            InitializeComponent();
        }

        private void CreateSettingsData()
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(filePath))
            {
                var defaultConfig = new
                {
                    Theme = 0,
                    TransparencyEffect = 0,
                    EnableUpdateChecks = 1,
                    RecordEndWaitTime = 75,
                    NotificationTimeout = 1750
                };

                var defaultJson = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, defaultJson); // File gets created automatically at this point
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }

    }
}
