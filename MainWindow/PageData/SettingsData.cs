using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using Whisper.net.Ggml;

#pragma warning disable MVVMTK0045
namespace AudioReplacer.MainWindow.PageData;
public partial class SettingsData : ObservableObject
{
    [ObservableProperty] private bool whisperAvailable = AppProperties.IsWhisperInstalled;
    [ObservableProperty] private int selectedAppTheme = App.AppSettings.AppThemeSetting;

    partial void OnSelectedAppThemeChanged(int value)
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
            rootElement.RequestedTheme = (ElementTheme) value;
        
        App.AppSettings.AppThemeSetting = value;
        if (value == 1) 
            SelectedTransparencyMode = 0;
    }

    [ObservableProperty]
    private int selectedTransparencyMode = App.AppSettings.AppTransparencySetting;
    partial void OnSelectedTransparencyModeChanged(int value)
    {
        switch (value)
        {
            case 0 when MicaController.IsSupported():
                App.MainWindow.SystemBackdrop = new MicaBackdrop();
                break;
            case 0 when !MicaController.IsSupported():
            default:
                App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
                break;
        }
        App.AppSettings.AppTransparencySetting = value;
    }

    [ObservableProperty] private bool randomizeFiles = AppFunctions.IntToBool(App.AppSettings.InputRandomizationEnabled);
    partial void OnRandomizeFilesChanged(bool value)
    {
        App.AppSettings.InputRandomizationEnabled = AppFunctions.BoolToInt(value);
    }

    [ObservableProperty] private bool enableRpc = AppFunctions.IntToBool(App.AppSettings.EnableRichPresence);
    partial void OnEnableRpcChanged(bool value)
    {
        App.AppSettings.EnableRichPresence = AppFunctions.BoolToInt(value);
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

    [ObservableProperty] private bool enableTranscription = AppFunctions.IntToBool(App.AppSettings.EnableTranscription);
    partial void OnEnableTranscriptionChanged(bool value)
    {
        App.AppSettings.EnableTranscription = AppFunctions.BoolToInt(value);
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
        Task.Run(async () => await AppFunctions.SpawnProcess("explorer", AppProperties.OutputPath));
    }

    [RelayCommand]
    private void OpenDataFile()
    {
        if (!AppProperties.IsAppLoaded) return;
        Task.Run(async () => await AppFunctions.SpawnProcess("", string.Empty));
    }

    [RelayCommand]
    private void ResetSettings()
    {
        if (!AppProperties.IsAppLoaded) return;
        File.Delete(AppProperties.SettingsFile);
        AppFunctions.RestartApp();
    }

    [RelayCommand]
    private void ResetCustomPitch()
    {
        if (!AppProperties.IsAppLoaded) return;
        File.Delete(AppProperties.PitchDataFile);
        AppFunctions.RestartApp();
    }

    [RelayCommand]
    private void ResetCustomEffects()
    {
        if (!AppProperties.IsAppLoaded) return;
        File.Delete(AppProperties.EffectsDataFile);
        AppFunctions.RestartApp();
    }

    [RelayCommand]
    private void ResetAll()
    {
        if (!AppProperties.IsAppLoaded) return;
        File.Delete(AppProperties.EffectsDataFile);
        File.Delete(AppProperties.PitchDataFile);
        File.Delete(AppProperties.SettingsFile);
        AppFunctions.RestartApp();
    }

    [RelayCommand]
    private async Task DownloadWhisper()
    {
        App.MainWindow.ShowProgressNotification("Downloading Data", "Do not disconnect from the internet. App will restart after download is completed");
        await using var whisperStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Small, QuantizationType.Q4_0);
        await using var fileWriter = File.OpenWrite(AppProperties.WhisperPath);
        await whisperStream.CopyToAsync(fileWriter);
        AppFunctions.RestartApp();
    }
}
