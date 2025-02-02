using AudioReplacer.Util;
using AudioReplacer.Windows.MainWindow;
using AudioReplacer.Windows.Setup;
using Config.Net;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AudioReplacer.Util.Logger;
using Velopack;
using WinRT.Interop;

namespace AudioReplacer;

public partial class App // I will admit, code-behind is still pretty useful here. Mvvm would overcomplicate things. While this is messier, it gets the job done
{
    public static AppWindow AppWindow;
    public static MainWindow MainWindow { get; private set; }
    public static FirstTimeSetupWindow SetupWindow { get; set; }
    public static IAppSettings AppSettings { get; private set; }
    public static RichPresenceController DiscordController;

    public App()
    {
        File.WriteAllText(Generic.LogFile, "Log Started!");
        CreateSettingsData();
        CreateJsonData();
        AppSettings = new ConfigurationBuilder<IAppSettings>().UseJsonFile(Generic.SettingsFile).Build();
        InitializeComponent();
        VelopackApp.Build().Run();
        if (!Directory.Exists(Generic.BinaryPath)) 
            Directory.CreateDirectory(Generic.BinaryPath);
    }

    private void CreateSettingsData()
    {
        if (!Directory.Exists(Generic.ConfigPath)) 
            Directory.CreateDirectory(Generic.ConfigPath);

        if (File.Exists(Generic.SettingsFile))
        {
            // Create missing properties in the settings json on updates
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
                { "RecordStartWaitTime", 25 },
                { "EnableTranscription", 1},
                { "AutoConvertFiles", 1 },
                { "OutputFileType", "wav"}
            };

            // Merge existing settings with default settings
            foreach (string key in defaultConfig.Keys.Where(key => !existingConfig.ContainsKey(key)))
            {
                existingConfig[key] = defaultConfig[key];
            }

            var updatedJson = JsonSerializer.Serialize(existingConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Generic.SettingsFile, updatedJson);
            return;
        }

        // Generate a new configuration file with the default values if the configuration file does not exist
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
            RecordStartWaitTime = 25,
            EnableTranscription = 1,
            AutoConvertFiles = 1,
            OutputFileType = "wav"
        };

        var defaultJson = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Generic.SettingsFile, defaultJson);
    }

    [Log]
    private void CreateJsonData()
    {
        // Users will have to import their own data files for this app to work,
        // but create an empty file with the start of a json array (for functionality purposes)
        if (!File.Exists(Generic.PitchDataFile)) File.WriteAllText(Generic.PitchDataFile, "[\n\n]");
        if (!File.Exists(Generic.EffectsDataFile)) File.WriteAllText(Generic.EffectsDataFile, "[\n\n]");

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
        Generic.PopulateCustomData();
    }

    [Log]
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        switch (File.Exists(Path.Join(Generic.ConfigPath, ".setupCompleted")))
        {
            case true:
                // Only initialize rich presence when app is configured
                if (Generic.IntToBool(App.AppSettings.EnableRichPresence)) 
                    DiscordController = new RichPresenceController(1325340097234866297, "On Record Page", "No Project Loaded", "idle", "Idle");
                MainWindow = new MainWindow();
                MainWindow.Activate();
                break;
            case false:
                SetupWindow = new FirstTimeSetupWindow();
                SetupWindow.Activate();
                break;
        }
        Generic.IsAppLoaded = true;
    }

    [Log]
    public static AppWindow GetAppWindowForCurrentWindow(object window) // Thanks StackOverflow man!
    {
        var hWnd = WindowNative.GetWindowHandle(window);
        var currentWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(currentWndId);
    }
}
