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
        private readonly string directoryPath, settingsFilePath, pitchDataPath;

        public App()
        {
            directoryPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Config";
            settingsFilePath = $"{directoryPath}\\AudioReplacer2-Config.json";
            pitchDataPath = $"{directoryPath}\\PitchData.json";
            CreateSettingsData();
            CreatePitchData();

            AppSettings = new ConfigurationBuilder<IAppSettings>()
                .UseJsonFile(settingsFilePath)
                .Build();

            GlobalData.updateChecksAllowed = AppSettings.AppUpdateCheck == 1;
            GlobalData.notificationTimeout = AppSettings.NotificationTimeout;
            GlobalData.recordStopDelay = AppSettings.RecordEndWaitTime;
            InitializeComponent();
        }

        private void CreateSettingsData()
        {
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath); 
            if (!File.Exists(settingsFilePath))
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
                File.WriteAllText(settingsFilePath, defaultJson); // File gets created automatically by File.WriteAllText() before it writes to anything
            }
        }

        private void CreatePitchData()
        {
            if (!File.Exists(pitchDataPath)) { File.WriteAllText(pitchDataPath, JsonSerializer.Serialize(GlobalData.pitchData, new JsonSerializerOptions { WriteIndented = true })); }
            GlobalData.deserializedPitchData = JsonSerializer.Deserialize<string[][]>(File.ReadAllText(pitchDataPath));
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
