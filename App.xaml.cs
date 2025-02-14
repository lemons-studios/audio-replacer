using AudioReplacer.Generic;
using AudioReplacer.Util;
using AudioReplacer.MainWindow;
using AudioReplacer.Setup;
using Config.Net;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Velopack;
using WinRT.Interop;

namespace AudioReplacer;

public partial class App // I will admit, code-behind is still pretty useful here. Mvvm would overcomplicate things. While this is messier, it gets the job done
{
    public static AppWindow AppWindow;
    public static MainWindow.MainWindow MainWindow { get; private set; }
    public static FirstTimeSetupWindow SetupWindow { get; set; }
    public static IAppSettings AppSettings { get; private set; }
    public static RichPresenceController DiscordController;

    public App()
    {
        File.WriteAllText(AppProperties.LogFile, "Log Started!");
        CreateSettingsData();
        CreateJsonData();
        AppSettings = new ConfigurationBuilder<IAppSettings>().UseJsonFile(AppProperties.SettingsFile).Build();
        InitializeComponent();
        VelopackApp.Build().Run();
        if (!Directory.Exists(AppProperties.BinaryPath)) 
            Directory.CreateDirectory(AppProperties.BinaryPath);
    }

    private void CreateSettingsData()
    {
        if (!Directory.Exists(AppProperties.ConfigPath))
            Directory.CreateDirectory(AppProperties.ConfigPath);

        if (!File.Exists(AppProperties.SettingsFile))
            File.WriteAllText(AppProperties.SettingsFile, "{\n\n}");

        var existingConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(AppProperties.SettingsFile));
        var defaultConfig = new Dictionary<string, object>
        {
            { "Theme", 0 },
            { "TransparencyEffect", AppFunctions.GetTransparencyMode() },
            { "EnableUpdateChecks", 1 },
            { "RecordEndWaitTime", 0 },
            { "NotificationTimeout", 1750 },
            { "RememberSelectedFolder", 1 },
            { "LastSelectedFolder", "" },
            { "InputRandomizationEnabled", 0 },
            { "RecordStartWaitTime", 25 },
            { "EnableTranscription", 1 },
            { "AutoConvertFiles", 1 },
            { "OutputFileType", "wav" }
        };

        // Merge existing settings with default settings
        foreach (string key in defaultConfig.Keys.Where(key => !existingConfig.ContainsKey(key)))
        {
            existingConfig[key] = defaultConfig[key];
        }

        var updatedJson = JsonSerializer.Serialize(existingConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(AppProperties.SettingsFile, updatedJson);
    }

    [Log]
    private void CreateJsonData()
    {
        // Users will have to import their own data files for this app to work,
        // but create an empty file with the start of a json array (for functionality purposes)
        if (!File.Exists(AppProperties.PitchDataFile)) 
            File.WriteAllText(AppProperties.PitchDataFile, "[\n\n]");

        if (!File.Exists(AppProperties.EffectsDataFile)) 
            File.WriteAllText(AppProperties.EffectsDataFile, "[\n\n]");

        try
        {
            AppProperties.PitchData =
                JsonSerializer.Deserialize<string[][]>(File.ReadAllText(AppProperties.PitchDataFile));
        }
        catch (JsonException)
        {
            AppProperties.PitchData = [];
        }

        try
        {
            AppProperties.EffectData = JsonSerializer.Deserialize<string[][]>(File.ReadAllText(AppProperties.EffectsDataFile));
        }
        catch (JsonException)
        {
            AppProperties.EffectData = [];
        }
        AppFunctions.PopulateCustomData();
    }

    [Log]
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        switch (File.Exists(Path.Join(AppProperties.ConfigPath, ".setupCompleted")))
        {
            case true:
                // Only initialize rich presence when app is configured
                if (AppFunctions.IntToBool(App.AppSettings.EnableRichPresence)) 
                    DiscordController = new RichPresenceController("On Record Page", "No Project Loaded", "idle", "Idle");
                MainWindow = new MainWindow.MainWindow();
                MainWindow.Activate();
                break;
            case false:
                SetupWindow = new FirstTimeSetupWindow();
                SetupWindow.Activate();
                break;
        }
        AppProperties.IsAppLoaded = true;
    }

    [Log]
    public static AppWindow GetAppWindowForCurrentWindow(object window) // Thanks StackOverflow man!
    {
        var hWnd = WindowNative.GetWindowHandle(window);
        var currentWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(currentWndId);
    }
}
