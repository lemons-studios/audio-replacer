using AudioReplacer.Generic;
using AudioReplacer.Util;
using Config.Net;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Text.Json;

namespace AudioReplacer
{
    public partial class App
    {
        public static IAppSettings AppSettings { get; private set; }
        public static MainWindow MainWindow { get; private set; }
        public static RichPresenceController DiscordController;

        private readonly string directoryPath, settingsFilePath, pitchDataPath, effectDataPath;

        public App()
        {
            directoryPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\audio-replacer\config";
            settingsFilePath = $@"{directoryPath}\AppSettings.json";
            pitchDataPath = $@"{directoryPath}\PitchData.json";
            effectDataPath = $@"{directoryPath}\EffectsData.json";

            CreateSettingsData();
            CreateJsonData();
            AppSettings = new ConfigurationBuilder<IAppSettings>().UseJsonFile(settingsFilePath).Build();

            SetGlobalData();
            DiscordController = new RichPresenceController(1325340097234866297, "On Record Page", "No Project Loaded", "idle", "idle");
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
                RecordEndWaitTime = 0,
                NotificationTimeout = 1750,
                RememberSelectedFolder = 1,
                LastSelectedFolder = "",
                InputRandomizationEnabled = 0,
                ShowEffectSelection = 0,
                EnableFanfare = 0,
                RecordStartWaitTime = 25
            };
            string defaultJson = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, defaultJson); // File gets created automatically by File.WriteAllText() before it writes to anything
        }

        private void CreateJsonData()
        {
            // Users will have to import their own data files for this app to work,
            // but create an empty file with the start of a json array (for functionality purposes)
            if (!File.Exists(pitchDataPath))
                File.WriteAllText(pitchDataPath, "[\n\n]");

            if (!File.Exists(effectDataPath))
                File.WriteAllText(effectDataPath, "[\n\n]");

            AppGeneric.PitchData = JsonSerializer.Deserialize<string[][]>(File.ReadAllText(pitchDataPath));
            AppGeneric.EffectData = JsonSerializer.Deserialize<string[][]>(File.ReadAllText(effectDataPath));
        }

        private void SetGlobalData()
        {
            AppGeneric.UpdateChecksAllowed = AppSettings.AppUpdateCheck == 1;
            AppGeneric.InputRandomizationEnabled = AppSettings.InputRandomizationEnabled == 1;
            AppGeneric.ShowAudioEffectDetails = AppSettings.ShowEffectSelection == 1;
            AppGeneric.EnableFanfare = AppSettings.EnableFanfare == 1;
            AppGeneric.NotificationTimeout = AppSettings.NotificationTimeout;
            AppGeneric.RecordStopDelay = AppSettings.RecordEndWaitTime;
            AppGeneric.RecordStartDelay = AppSettings.RecordStartWaitTime;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
