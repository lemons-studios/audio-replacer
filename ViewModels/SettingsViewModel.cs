using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using System.Threading.Tasks;
using AudioReplacer.Util;

namespace AudioReplacer.Util;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private int selectedAppTheme = App.AppSettings.AppThemeSetting;
    partial void OnSelectedAppThemeChanged(int value)
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = (ElementTheme) value;
        }
        App.AppSettings.AppThemeSetting = value;
    }

    [ObservableProperty]
    private int selectedTransparencyMode = App.AppSettings.AppTransparencySetting;

 
    partial void OnSelectedTransparencyModeChanged(int value)
    {
        if (value == 0 && MicaController.IsSupported())
        {
            App.MainWindow.SystemBackdrop = new MicaBackdrop();
        }
        else
        {
            App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
            if (value == 0) // Ensure the value is set to 1 only if it was 0
            {
                SelectedTransparencyMode = 1;
            }
        }

        App.AppSettings.AppTransparencySetting = value;
    }

    [ObservableProperty] private bool enableUpdateChecks = App.AppSettings.AppUpdateCheck == 1;
    partial void OnEnableUpdateChecksChanged(bool value)
    {
        App.AppSettings.AppUpdateCheck = Generic.BoolToInt(value);
    }

    [ObservableProperty] private bool enableFolderMemory = App.AppSettings.RememberSelectedFolder == 1;
    partial void OnEnableFolderMemoryChanged(bool value)
    {
        App.AppSettings.RememberSelectedFolder = Generic.BoolToInt(value);
    }

    [ObservableProperty] private bool randomizeFiles = App.AppSettings.InputRandomizationEnabled == 1;
    partial void OnRandomizeFilesChanged(bool value)
    {
        App.AppSettings.InputRandomizationEnabled = Generic.BoolToInt(value);
    }

    [ObservableProperty] private bool enableVerbosePitch = App.AppSettings.ShowEffectSelection == 1;
    partial void OnEnableVerbosePitchChanged(bool value)
    {
        App.AppSettings.ShowEffectSelection = Generic.BoolToInt(value);
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
        Task.Run(async () => await Generic.SpawnProcess("explorer", Path.Combine(Generic.extraApplicationData, "out")));
    }

    [RelayCommand]
    private void OpenDataFile()
    {
        if (!Generic.isAppLoaded) return;
        Task.Run(async () => await Generic.SpawnProcess("", string.Empty));
    }

    [RelayCommand]
    private void ResetSettings()
    {
        if (!Generic.isAppLoaded) return;
        File.Delete(Generic.SettingsFile);
        Generic.RestartApp();
    }

    [RelayCommand]
    private void ResetCustomPitch()
    {
        if (!Generic.isAppLoaded) return;
        File.Delete(Generic.PitchDataFile);
        Generic.RestartApp();
    }

    [RelayCommand]
    private void ResetCustomEffects()
    {
        if (!Generic.isAppLoaded) return;
        File.Delete(Generic.EffectsDataFile);
        Generic.RestartApp();
    }

    [RelayCommand]
    private void ResetAll()
    {
        if (!Generic.isAppLoaded) return;
        File.Delete(Generic.EffectsDataFile);
        File.Delete(Generic.PitchDataFile);
        File.Delete(Generic.SettingsFile); // TODO: Check if settings also included
        Generic.RestartApp();
    }
}