using AudioReplacer.Util;
using AudioReplacer.Windows.FirstTimeSetup.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Animation;
using SevenZipExtractor;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Whisper.net.Ggml;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AudioReplacer.Windows.FirstTimeSetup;

public partial class SetupData : ObservableObject
{
    // All properties must be static so all setup pages will have the correct value (since they are separate instances of the class)
    [ObservableProperty] private static int recordStartDelay, recordEndDelay;
    [ObservableProperty] private static bool checkForUpdates = true, richPresenceEnabled = true, enableFileRandomization, downloadWhisper = true;
    [ObservableProperty] private static string pitchSettingsPath, effectSettingsPath;
    private static int currentSetupStep;

    [RelayCommand]
    private void NextPage()
    {
        currentSetupStep += 1;
        Type[] steps = [typeof(SetupWelcome), typeof(SetupSettings), typeof(SetupAdvanced), typeof(SetupDownloading)];
        App.SetupWindow.GetMainFrame().Navigate(steps[currentSetupStep], null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
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

    [RelayCommand]
    private async Task DownloadData()
    {
        try
        {
            // Before downloading, first import any data files the user wanted to import
            if (PitchSettingsPath != string.Empty && File.Exists(PitchSettingsPath))
                File.Copy(PitchSettingsPath, Generic.PitchDataFile, overwrite: true);

            if (EffectSettingsPath != string.Empty && File.Exists(EffectSettingsPath))
                File.Copy(EffectSettingsPath, Generic.EffectsDataFile, overwrite: true);

            // Set App Settings
            var appSettings = App.AppSettings;
            appSettings.RecordStartWaitTime = Math.Max(0, RecordStartDelay);
            appSettings.RecordEndWaitTime = Math.Max(0, RecordEndDelay);
            appSettings.EnableRichPresence = Generic.BoolToInt(RichPresenceEnabled);
            appSettings.InputRandomizationEnabled = Generic.BoolToInt(EnableFileRandomization);

            // Download FFmpeg
            var latestVersion = await Generic.GetWebData("https://www.gyan.dev/ffmpeg/builds/release-version");
            var ffmpegUrl = $"https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-{latestVersion}-full_build.7z";
            var outPath = Path.Combine(Generic.extraApplicationData, "ffmpeg");
            Generic.DownloadFile(ffmpegUrl, $"{outPath}.7z");
            // Extract ffmpeg 7z file, move contents of "bin" folder to the App's bin folder, and delete both the archive and the extracted files after moving everything over
            using (ArchiveFile ffmpegExtractor = new ArchiveFile($"{outPath}.7z")) ffmpegExtractor.Extract(outPath);

            DirectoryInfo info = new DirectoryInfo(outPath);
            // Move all files of type exe to the app binaries folder 
            foreach (var exe in info.GetFiles("*.exe", SearchOption.AllDirectories))
            {
                var path = exe.ToString();
                File.Move(path, Path.Combine(Generic.binaryPath, exe.Name));
            }

            Directory.Delete(outPath, true);
            File.Delete($"{outPath}.7z");

            // If the whisper download is enabled, download whisper.NET model (q5_1/ggml-small.bin)
            if (DownloadWhisper)
            {
                // Yoinked directly from the whisper.net GitHub page
                await using var whisperStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Small, QuantizationType.Q5_1);
                await using var fileWriter = File.OpenWrite(Generic.whisperPath);
                await whisperStream.CopyToAsync(fileWriter);
            }

            // Finally, mark the app as "set up" in the app settings, and restart the app
            App.AppSettings.AppConfigured = 1;
            Generic.RestartApp();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
}