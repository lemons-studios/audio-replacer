using AudioReplacer.Util;
using Config.Net;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.UI.Windowing;
using Velopack;
using Microsoft.UI;
using System;
using System.Threading.Tasks;
using WinRT.Interop;

namespace AudioReplacer
{
    public partial class App // I will admit, code-behind is still pretty useful here. Mvvm would overcomplicate things. While this is messier, it gets the job done
    {
        public static AppWindow AppWindow;
        public static MainWindow MainWindow { get; private set; }
        public static IAppSettings AppSettings { get; private set; }
        public static RichPresenceController DiscordController;

        public App()
        {
            CreateSettingsData();
            CreateJsonData();
            AppSettings = new ConfigurationBuilder<IAppSettings>().UseJsonFile(Generic.SettingsFile).Build();
            DiscordController = new RichPresenceController(1325340097234866297, "On Record Page", "No Project Loaded", "idle", "Idle");
            InitializeComponent();
            VelopackApp.Build().Run();
        }

        private void CreateSettingsData()
        {
            if (!Directory.Exists(Generic.configPath)) Directory.CreateDirectory(Generic.configPath);
            if (File.Exists(Generic.SettingsFile))
            {
                // Load existing settings
                var existingConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(Generic.SettingsFile));
                var defaultConfig = new Dictionary<string, object>
                {
                    { "Theme", 0 },
                    { "TransparencyEffect", Generic.GetTransparencyMode() },
                    { "EnableUpdateChecks", 1 },
                    { "RecordEndWaitTime", 0 },
                    { "NotificationTimeout", 1750 },
                    { "RememberSelectedFolder", 1 },
                    { "LastSelectedFolder", "" },
                    { "InputRandomizationEnabled", 0 },
                    { "ShowEffectSelection", 0 },
                    { "RecordStartWaitTime", 25 }
                };

                // Merge existing settings with default settings
                foreach (string key in defaultConfig.Keys.Where(key => !existingConfig.ContainsKey(key)))
                {
                    existingConfig[key] = defaultConfig[key];
                }
                
                string updatedJson = JsonSerializer.Serialize(existingConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Generic.SettingsFile, updatedJson);
                return;
            }

            var newConfig = new
            {
                Theme = 0,
                TransparencyEffect = Generic.GetTransparencyMode(), // Prevents mica being set on Windows 10 devices
                EnableUpdateChecks = 1,
                RecordEndWaitTime = 0,
                NotificationTimeout = 1750,
                RememberSelectedFolder = 1,
                LastSelectedFolder = "",
                InputRandomizationEnabled = 0,
                ShowEffectSelection = 0,
                RecordStartWaitTime = 25
            };
            string defaultJson = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Generic.SettingsFile, defaultJson);
        }

        private void CreateJsonData()
        {
            // Users will have to import their own data files for this app to work,
            // but create an empty file with the start of a json array (for functionality purposes)
            if (!File.Exists(Generic.PitchDataFile))
                File.WriteAllText(Generic.PitchDataFile, "[\n\n]");

            if (!File.Exists(Generic.EffectsDataFile))
                File.WriteAllText(Generic.EffectsDataFile, "[\n\n]");

            try
            {
                Generic.PitchData = JsonSerializer.Deserialize<string[][]>(File.ReadAllText(Generic.PitchDataFile));
            }
            catch (JsonException)
            {
                Generic.PitchData = [];
            }
            try
            {
                Generic.EffectData = JsonSerializer.Deserialize<string[][]>(File.ReadAllText(Generic.EffectsDataFile));
            }
            catch (JsonException)
            {
                Generic.EffectData = [];
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Generic.PopulateCustomData();
            MainWindow = new MainWindow();
            MainWindow.Activate();
            Generic.isAppLoaded = true;
            Task.Run(AppUpdater.AreUpdatesAvailable);
        }

        public static AppWindow GetAppWindowForCurrentWindow(object window) // Thanks StackOverflow man!
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            var currentWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(currentWndId);
        }
    }
}