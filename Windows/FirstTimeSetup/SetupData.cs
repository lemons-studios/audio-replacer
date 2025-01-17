using System;
using System.Collections.Generic;
using AudioReplacer.Util;
using AudioReplacer.Windows.FirstTimeSetup.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace AudioReplacer.Windows.FirstTimeSetup;

public partial class SetupData : ObservableObject
{
    [ObservableProperty] private int recordStartDelay, recordEndDelay;
    [ObservableProperty] private bool richPresenceEnabled = true, enableFileRandomization, downloadWhisper = true;
    [ObservableProperty] private string pitchSettingsPath, effectSettingsPath;
    [ObservableProperty] private float downloadPercentage;
    [ObservableProperty] private int currentSetupStep = 0;

    [RelayCommand]
    private void NextPage()
    {
        CurrentSetupStep += 1;
        Type[] steps = [typeof(SetupWelcome), typeof(SetupSettings), typeof(SetupAdvanced), typeof(SetupDownloading)];
        App.SetupWindow.GetMainFrame().Navigate(steps[CurrentSetupStep]);
    }

    [RelayCommand]
    private void EndSetup()
    {
        App.AppSettings.AppConfigured = 1;
        Generic.RestartApp();
    }
}