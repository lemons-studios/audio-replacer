using System;
using System.IO;
using System.Text.Json;
using AudioReplacer.Util;
using Config.Net;
using Microsoft.UI.Xaml;

namespace AudioReplacer
{
    public partial class App
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
            GlobalData.UpdateChecksAllowed = AppSettings.AppUpdateCheck == 1;
            GlobalData.NotificationTimeout = AppSettings.NotificationTimeout;
            GlobalData.RecordStopDelay = AppSettings.RecordEndWaitTime;
            InitializeComponent();
        }

        private void CreateSettingsData()
        {
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            if (File.Exists(settingsFilePath)) return;
            var defaultConfig = new
            {
                Theme = 0,
                TransparencyEffect = 0,
                EnableUpdateChecks = 1,
                RecordEndWaitTime = 25,
                NotificationTimeout = 1750
            };
            string defaultJson = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, defaultJson); // File gets created automatically by File.WriteAllText() before it writes to anything
        }

        private void CreatePitchData()
        {
            if (!File.Exists(pitchDataPath)) { File.WriteAllText(pitchDataPath, JsonSerializer.Serialize(GlobalData.pitchData, new JsonSerializerOptions { WriteIndented = true })); }
            GlobalData.DeserializedPitchData = JsonSerializer.Deserialize<string[][]>(File.ReadAllText(pitchDataPath));
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
