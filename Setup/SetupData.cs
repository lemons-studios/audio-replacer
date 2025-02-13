using AudioReplacer.Generic;
using AudioReplacer.Setup.Pages;
using AudioReplacer.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using SevenZipExtractor;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Whisper.net.Ggml;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AudioReplacer.Setup;
public partial class SetupData : ObservableObject
{
    // All properties must be static so all setup pages will have the correct value (since they are separate instances of the class)
    [ObservableProperty] private static bool downloadWhisper = true;
    [ObservableProperty] private static string pitchSettingsPath, effectSettingsPath, stepStatus;
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
        if (!string.IsNullOrWhiteSpace(PitchSettingsPath) && File.Exists(PitchSettingsPath)) 
            File.Copy(PitchSettingsPath, AppProperties.PitchDataFile, overwrite: true);
        if (!string.IsNullOrWhiteSpace(EffectSettingsPath) && File.Exists(EffectSettingsPath))
            File.Copy(EffectSettingsPath, AppProperties.EffectsDataFile, overwrite: true);
        
        // Download FFmpeg
        var latestVersion = await AppFunctions.GetWebData("https://www.gyan.dev/ffmpeg/builds/release-version");
        var ffmpegUrl = $"https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-{latestVersion}-full_build.7z";
        var outPath = Path.Join(AppProperties.ExtraApplicationData, "ffmpeg");
        await AppFunctions.DownloadFileAsync(ffmpegUrl, $@"{AppProperties.ExtraApplicationData}\ffmpeg.7z");
        
        // Extract FFmpeg
        using (var ffmpegExtractor = new ArchiveFile($"{outPath}.7z")) ffmpegExtractor.Extract(outPath);

        // Move FFmpeg executable (ffmpeg.exe ONLY, ffprobe.exe and ffplay.exe are not needed) to the application's binary folder
        var info = new DirectoryInfo(outPath);
        foreach (var exe in info.GetFiles("ffmpeg.exe", SearchOption.AllDirectories))
        {
            File.Move(exe.FullName, Path.Combine(AppProperties.BinaryPath, exe.Name));
        }

        Directory.Delete(outPath, true);
        File.Delete($"{outPath}.7z");

        // Download VgmStream
        var latestVgmStream = await AppFunctions.GetDataFromGithub("https://api.github.com/repos/vgmstream/vgmstream/releases/latest", "tag_name");
        var fullUrl = $"https://github.com/vgmstream/vgmstream/releases/download/{latestVgmStream}/vgmstream-win64.zip";
        await AppFunctions.DownloadFileAsync(fullUrl, AppProperties.BinaryPath);
        ZipFile.ExtractToDirectory(Path.Join(AppProperties.BinaryPath, "vgmstream-win64.zip"), AppProperties.BinaryPath);

        // Download Whisper model if enabled
        if (DownloadWhisper)
        {
            await using var whisperStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Small, QuantizationType.Q5_1);
            await using var fileWriter = File.OpenWrite(AppProperties.WhisperPath);
            await whisperStream.CopyToAsync(fileWriter);
        }
        // Mark the app as "set up" and restart the application
        // Ok I have no clue I decided to write a file instead of creating a property inside the settings file, but I guess it works
        File.Create(Path.Join(AppProperties.ConfigPath, ".setupCompleted"));
        AppFunctions.RestartApp();
    }

    private readonly Type[] setupPages = [typeof(SetupWelcome), typeof(SetupSettings), typeof(SetupAdvanced), typeof(SetupDownloading)]; // Increases efficiency when switching setup pages
    [RelayCommand]
    private void NextPage()
    {
        currentSetupStep += 1;
        App.SetupWindow.GetMainFrame().Navigate(setupPages[currentSetupStep], null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });

        if (currentSetupStep == 3) 
            Task.Run(DownloadData);
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
        if (App.SetupWindow.Content is FrameworkElement rootElement) rootElement.RequestedTheme = (ElementTheme) value;
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
        App.AppSettings.AppUpdateCheck = AppFunctions.BoolToInt(value);
    }

    [ObservableProperty] private static bool richPresenceEnabled = true;
    partial void OnRichPresenceEnabledChanged(bool value)
    {
        App.AppSettings.EnableRichPresence = AppFunctions.BoolToInt(value);
    }

    [ObservableProperty] private static bool enableFileRandomization;
    partial void OnEnableFileRandomizationChanged(bool value)
    {
        App.AppSettings.InputRandomizationEnabled = AppFunctions.BoolToInt(value);
    }
}
