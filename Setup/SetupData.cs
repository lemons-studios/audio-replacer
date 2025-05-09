﻿using Windows.Storage.Pickers;
using AudioReplacer.Setup.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Whisper.net.Ggml;
using WinRT.Interop;
#pragma warning disable MVVMTK0045

namespace AudioReplacer.Setup;
public partial class SetupData : ObservableObject
{
    // All properties must be static so all setup pages will have the correct value (since they are separate instances of the class)
    
    [ObservableProperty] private static bool downloadWhisper = true;
    [ObservableProperty] private static string pitchSettingsPath, effectSettingsPath, stepStatus;
    private static int currentSetupStep;
    private static readonly Type[] SetupPages = [typeof(SetupWelcome), typeof(SetupSettings), typeof(SetupAdvanced), typeof(SetupDownloading)]; // Increases efficiency when switching setup pages

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

        // Download Whisper model if enabled
        if (DownloadWhisper)
        {
            await using var whisperStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Small, QuantizationType.Q4_0);
            await using var fileWriter = File.OpenWrite(AppProperties.WhisperPath);
            await whisperStream.CopyToAsync(fileWriter);
        }

        App.AppSettings.SetupCompleted = 1;
        AppFunctions.RestartApp();
    }

    [RelayCommand]
    private void NextPage()
    {
        currentSetupStep += 1;
        App.SetupWindow.GetMainFrame().Navigate(SetupPages[currentSetupStep], null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });

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
