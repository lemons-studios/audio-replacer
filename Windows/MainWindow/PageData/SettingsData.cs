using AudioReplacer.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net.Ggml;

namespace AudioReplacer.Windows.MainWindow.PageData;

public partial class SettingsData : ObservableObject
{
    [ObservableProperty] private bool whisperAvailable = Generic.IsWhisperInstalled;
    [ObservableProperty] private bool whisperInstalled = !Generic.IsWhisperInstalled; // Bad variable name lol. should have just used inverse of whisperAvailable here
    [ObservableProperty] private int selectedAppTheme = App.AppSettings.AppThemeSetting;

    partial void OnSelectedAppThemeChanged(int value)
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
            rootElement.RequestedTheme = (ElementTheme) value;
        App.AppSettings.AppThemeSetting = value;
    }

    [ObservableProperty]
    private int selectedTransparencyMode = App.AppSettings.AppTransparencySetting;
    partial void OnSelectedTransparencyModeChanged(int value)
    {
        if (value == 0 && MicaController.IsSupported())
            App.MainWindow.SystemBackdrop = new MicaBackdrop();
        else
        {
            App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
            if (value == 0) // Ensure the value is set to 1 only if it was 0
                SelectedTransparencyMode = 1;
        }

        App.AppSettings.AppTransparencySetting = value;
    }

    [ObservableProperty] private bool enableUpdateChecks = Generic.IntToBool(App.AppSettings.AppUpdateCheck);
    partial void OnEnableUpdateChecksChanged(bool value)
    {
        App.AppSettings.AppUpdateCheck = Generic.BoolToInt(value);
    }

    [ObservableProperty] private bool enableFolderMemory = Generic.IntToBool(App.AppSettings.RememberSelectedFolder);
    partial void OnEnableFolderMemoryChanged(bool value)
    {
        App.AppSettings.RememberSelectedFolder = Generic.BoolToInt(value);
    }

    [ObservableProperty] private bool randomizeFiles = Generic.IntToBool(App.AppSettings.InputRandomizationEnabled);
    partial void OnRandomizeFilesChanged(bool value)
    {
        App.AppSettings.InputRandomizationEnabled = Generic.BoolToInt(value);
    }

    [ObservableProperty] private bool enableRpc = Generic.IntToBool(App.AppSettings.EnableRichPresence);
    partial void OnEnableRpcChanged(bool value)
    {
        App.AppSettings.EnableRichPresence = Generic.BoolToInt(value);
        switch (value)
        {
            case true:
                App.DiscordController.CreateRichPresence();
                break;
            case false:
                App.DiscordController.DisposeRpc();
                break;
        }
    }

    [ObservableProperty] private bool enableTranscription = Generic.IntToBool(App.AppSettings.EnableTranscription);
    partial void OnEnableTranscriptionChanged(bool value)
    {
        App.AppSettings.EnableTranscription = Generic.BoolToInt(value);
    }

    [ObservableProperty] private int notificationStayTime = App.AppSettings.NotificationTimeout;
    partial void OnNotificationStayTimeChanged(int value)
    {
        App.AppSettings.NotificationTimeout = Math.Max(value, 500);
    }

    [ObservableProperty] private int recordStopDelay = App.AppSettings.RecordEndWaitTime;
    partial void OnRecordStopDelayChanged(int value)
    {
        App.AppSettings.RecordEndWaitTime = Math.Max(value, 0);
    }

    [ObservableProperty] private int recordStartDelay = App.AppSettings.RecordStartWaitTime;
    partial void OnRecordStartDelayChanged(int value)
    {
        App.AppSettings.RecordStartWaitTime = Math.Max(value, 0);
    }

    [RelayCommand]
    private void OpenOutputFolder()
    {
        Task.Run(async () => await Generic.SpawnProcess("explorer", Path.Combine(Generic.ExtraApplicationData, "out")));
    }

    [RelayCommand]
    private void OpenDataFile()
    {
        if (!Generic.IsAppLoaded) return;
        Task.Run(async () => await Generic.SpawnProcess("", string.Empty));
    }

    [RelayCommand]
    private void ResetSettings()
    {
        if (!Generic.IsAppLoaded) return;
        File.Delete(Generic.SettingsFile);
        Generic.RestartApp();
    }

    [RelayCommand]
    private void ResetCustomPitch()
    {
        if (!Generic.IsAppLoaded) return;
        File.Delete(Generic.PitchDataFile);
        Generic.RestartApp();
    }

    [RelayCommand]
    private void ResetCustomEffects()
    {
        if (!Generic.IsAppLoaded) return;
        File.Delete(Generic.EffectsDataFile);
        Generic.RestartApp();
    }

    [RelayCommand]
    private void ResetAll()
    {
        if (!Generic.IsAppLoaded) return;
        File.Delete(Generic.EffectsDataFile);
        File.Delete(Generic.PitchDataFile);
        File.Delete(Generic.SettingsFile);
        Generic.RestartApp();
    }

    [RelayCommand]
    private async Task DownloadWhisper()
    {
        App.MainWindow.ToggleProgressNotification("Downloading Data", "Do not disconnect from the internet. App will restart after download is completed");
        await using var whisperStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Small, QuantizationType.Q5_1);
        await using var fileWriter = File.OpenWrite(Generic.WhisperPath);
        await whisperStream.CopyToAsync(fileWriter);
        Generic.RestartApp();
    }
}
