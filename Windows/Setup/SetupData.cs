using AudioReplacer.Util;
using AudioReplacer.Util.Logger;
using AudioReplacer.Windows.Setup.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using SevenZipExtractor;
using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net.Ggml;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AudioReplacer.Windows.Setup;
public partial class SetupData : ObservableObject
{
    // All properties must be static so all setup pages will have the correct value (since they are separate instances of the class)
    [ObservableProperty] private static bool downloadWhisper = true;
    [ObservableProperty] private static string pitchSettingsPath, effectSettingsPath;
    private static int currentSetupStep;

    private async Task ImportData(bool isPitchFile)
    {
        var openPicker = new FileOpenPicker();
        InitializeWithWindow.Initialize(openPicker, WindowNative.GetWindowHandle(App.SetupWindow));
        openPicker.FileTypeFilter.Add(".json");
        var file = await openPicker.PickSingleFileAsync();
        if (file != null)
        {
            switch (isPitchFile)
            {
                case true:
                    PitchSettingsPath = file.Path;
                    break;
                case false:
                    EffectSettingsPath = file.Path;
                    break;
            }
        }
    }

    [Log]
    private async Task DownloadData()
    {
        // Before downloading, first import any data files the user wanted to import
        if (!string.IsNullOrWhiteSpace(PitchSettingsPath) && File.Exists(PitchSettingsPath))
        {
            File.Copy(PitchSettingsPath, Generic.PitchDataFile, overwrite: true);
        }

        if (!string.IsNullOrWhiteSpace(EffectSettingsPath) && File.Exists(EffectSettingsPath))
        {
            File.Copy(EffectSettingsPath, Generic.EffectsDataFile, overwrite: true);
        }

        // Download FFmpeg
        var latestVersion = await Generic.GetWebData("https://www.gyan.dev/ffmpeg/builds/release-version");
        var ffmpegUrl = $"https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-{latestVersion}-full_build.7z";
        var outPath = Path.Join(Generic.ExtraApplicationData, "ffmpeg");

        await Generic.DownloadFileAsync(ffmpegUrl, $@"{Generic.ExtraApplicationData}\ffmpeg.7z");
        // Extract FFmpeg
        using (var ffmpegExtractor = new ArchiveFile($"{outPath}.7z"))
        {
            ffmpegExtractor.Extract(outPath);
        }

        var info = new DirectoryInfo(outPath);

        // Move FFmpeg executables to the application's binary folder
        foreach (var exe in info.GetFiles("ffmpeg.exe", SearchOption.AllDirectories))
        {
            File.Move(exe.FullName, Path.Combine(Generic.BinaryPath, exe.Name));
        }

        Directory.Delete(outPath, true);
        File.Delete($"{outPath}.7z");

        // Download Whisper model if enabled
        if (DownloadWhisper)
        {
            await using var whisperStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Small, QuantizationType.Q5_1);
            await using var fileWriter = File.OpenWrite(Generic.WhisperPath);
            await whisperStream.CopyToAsync(fileWriter);
        }

        // Mark the app as "set up" and restart the application
        File.Create(Path.Join(Generic.ConfigPath, ".setupCompleted"));
        Generic.RestartApp();
    }

    [RelayCommand]
    private void NextPage()
    {
        currentSetupStep += 1;
        Type[] steps = [typeof(SetupWelcome), typeof(SetupSettings), typeof(SetupAdvanced), typeof(SetupDownloading)];
        App.SetupWindow.GetMainFrame().Navigate(steps[currentSetupStep], null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
        if (currentSetupStep == 3)
        {
            Task.Run(DownloadData);
        }
    }

    [RelayCommand]
    private async Task ImportPitchData()
    {
        await ImportData(true);
    }

    [RelayCommand]
    private async Task ImportEffectData()
    {
        await ImportData(false);
    }

    [ObservableProperty] private static int appTheme;
    partial void OnAppThemeChanged(int value)
    {
        if (App.SetupWindow.Content is FrameworkElement rootElement)
            rootElement.RequestedTheme = (ElementTheme) value;
        App.AppSettings.AppThemeSetting = value;
    }

    [ObservableProperty] private static int recordStartDelay = 20;
    partial void OnRecordStartDelayChanged(int value)
    {
        App.AppSettings.RecordStartWaitTime = value;
    }

    
    [ObservableProperty] private static int recordEndDelay = 20;
    partial void OnRecordEndDelayChanged(int value)
    {
        App.AppSettings.RecordEndWaitTime = value;
    }

    [ObservableProperty] private static bool checkForUpdates = true;
    partial void OnCheckForUpdatesChanged(bool value)
    {
        App.AppSettings.AppUpdateCheck = Generic.BoolToInt(value);
    }

    [ObservableProperty] private static bool richPresenceEnabled = true;
    partial void OnRichPresenceEnabledChanged(bool value)
    {
        App.AppSettings.EnableRichPresence = Generic.BoolToInt(value);
    }

    [ObservableProperty] private static bool enableFileRandomization;
    partial void OnEnableFileRandomizationChanged(bool value)
    {
        App.AppSettings.InputRandomizationEnabled = Generic.BoolToInt(value);
    }
}
