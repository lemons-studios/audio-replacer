﻿using System.Linq;
using System.Text.Json;
using AudioReplacer.Setup;
using Config.Net;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Velopack;
using WinRT.Interop;

namespace AudioReplacer;

/// <summary>
/// App Entry Point. Also stores commonly used elements from the app
/// </summary>
public partial class App
{
    public static AppWindow AppWindow;
    public static MainWindow.MainWindow MainWindow { get; private set; }
    public static FirstTimeSetupWindow SetupWindow { get; private set; }
    public static IAppSettings AppSettings { get; private set; }
#pragma warning disable CA2211
    public static RichPresenceController DiscordController;
#pragma warning restore CA2211

    public App()
    {
        CreateAdditionalData();
        CreateSettingsData();
        CreateJsonData();

        File.WriteAllText(AppProperties.LogFile, "Log Started!");
        AppSettings = new ConfigurationBuilder<IAppSettings>()
            .UseJsonFile(AppProperties.SettingsFile)
            .Build();

        DoVersionMigration();
        InitializeComponent();
        VelopackApp.Build().Run();
    }

    private void DoVersionMigration()
    {
        // Migration for versions below 4.0
        var legacyDataFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "audio-replacer");
        if(Directory.Exists(legacyDataFolder))
            Directory.Move(legacyDataFolder, AppProperties.ExtraApplicationData);

        // Migration for versions below 4.3.1
        var legacySetupCompleteCheck = Path.Join(AppProperties.ConfigPath, ".setupCompleted");
        if (File.Exists(legacySetupCompleteCheck))
        {
            File.Delete(legacySetupCompleteCheck);
            var hasSetupCompleted = File.Exists(legacySetupCompleteCheck) ? 1 : 0;
            AppSettings.SetupCompleted = hasSetupCompleted; // This will hopefully not launch app setup when updating
        }

        // Migration for versions below 4.3.2
        var legacyBinaryPath = Path.Join(AppProperties.ExtraApplicationData, "bin");
        if (Directory.Exists(legacyBinaryPath))
        {
            var legacyWhisperPath = Path.Join(legacyBinaryPath, "whisper.bin");
            if (File.Exists(legacyWhisperPath))
                File.Move(legacyWhisperPath, AppProperties.WhisperPath);
            Directory.Delete(legacyBinaryPath, true);
        }
    }

    private void CreateSettingsData()
    {

        Dictionary<string, object> existingConfig;
        try
        {
            existingConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(AppProperties.SettingsFile));
        }
        catch (JsonException)
        {
            existingConfig = new Dictionary<string, object>(); // Create a dummy dictionary if the json serializer fails (Usually when the file is empty or just being created)
        }

        var defaultConfig = new Dictionary<string, object>
        {
            { "Theme", 0 },
            { "TransparencyEffect", AppFunctions.GetTransparencyMode() },
            { "RecordEndWaitTime", 0 },
            { "NotificationTimeout", 1750 },
            { "LastSelectedFolder", "" },
            { "InputRandomizationEnabled", 0 },
            { "RecordStartWaitTime", 25 },
            { "EnableTranscription", 1 },
            { "SetupCompleted", 0 }
        };

        // Merge existing settings with default settings
        foreach (var k in defaultConfig.Keys.Where(e => !existingConfig.ContainsKey(e)))
        {
            existingConfig[k] = defaultConfig[k];
        }

        // This method only gets called on app launch, so I'm not too worried about it being "inefficient"
#pragma warning disable CA1869
        var updatedJson = JsonSerializer.Serialize(existingConfig, new JsonSerializerOptions { WriteIndented = true });
#pragma warning restore CA1869
        
        File.WriteAllText(AppProperties.SettingsFile, updatedJson);
    }

    [Log]
    private void CreateJsonData()
    {
        AppProperties.PitchData = LoadJsonData(AppProperties.PitchDataFile);
        AppProperties.EffectData = LoadJsonData(AppProperties.EffectsDataFile);
        AppFunctions.PopulateCustomData();
    }

    private string[][] LoadJsonData(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<string[][]>(File.ReadAllText(path));
        }
        catch (JsonException)
        {
            return [];
        }
    }
    
    [Log]
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        switch (AppFunctions.IntToBool(AppSettings.SetupCompleted))
        {
            case true:
                // Only initialize rich presence when app is configured
                if (AppFunctions.IntToBool(AppSettings.EnableRichPresence)) DiscordController = new RichPresenceController("Home Page", "", "", "");

                MainWindow = new MainWindow.MainWindow();
                MainWindow.Activate();
                break;
            case false:
                // No rich presence since it's unknown weather the user wants rich presence or not
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
    
    [Log]
    private void CreateAdditionalData()
    {
        string[] directories = [AppProperties.ExtraApplicationData, AppProperties.ConfigPath, AppProperties.OutputPath];
        string[] files = [AppProperties.SettingsFile, AppProperties.PitchDataFile, AppProperties.EffectsDataFile];

        foreach (var d in directories)
        {
            if (!Directory.Exists(d))
                Directory.CreateDirectory(d!);
        }

        foreach (var f in files)
        {
            // All additional files use the json format, so I will just have this method create a file with the square brackets
            if(!File.Exists(f))
                File.WriteAllText(f!, "[\n\n]");
        }
    }
}
