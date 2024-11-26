using System;
using System.Diagnostics;
using System.IO;
using AudioReplacer2.Util;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinUIEx;

namespace AudioReplacer2.Pages
{
    public sealed partial class SettingsPage
    {
        private readonly bool firstOpening;
        private readonly string configFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Config";

        public SettingsPage()
        {
            InitializeComponent();
            // Initialize all the data
            ThemeDropdown.SelectedIndex = App.AppSettings.AppThemeSetting;
            TransparencyDropdown.SelectedIndex = App.AppSettings.AppTransparencySetting;
            UpdateCheckSwitch.IsOn = GlobalData.UpdateChecksAllowed;
            ToastDelayBox.Value = GlobalData.NotificationTimeout;
            RecordDelayBox.Value = GlobalData.RecordStopDelay;

            firstOpening = false;
        }

        private void ToggleUpdateChecks(object sender, RoutedEventArgs e)
        {
            if (firstOpening) return;
            GlobalData.UpdateChecksAllowed = UpdateCheckSwitch.IsOn;
            App.AppSettings.AppUpdateCheck = BoolToInt(UpdateCheckSwitch.IsOn);
        }

        private void ToggleTransparencyMode(object sender, SelectionChangedEventArgs e)
        {
            if (firstOpening) return;

            switch (TransparencyDropdown.SelectedIndex)
            {
                case 0: // Mica Backdrop
                    if (MicaController.IsSupported())
                    {
                        App.MainWindow.SystemBackdrop = new MicaBackdrop();
                    }
                    else
                    {
                        App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
                        TransparencyDropdown.SelectedIndex = 1; // If mica isn't supported, switch to acrylic
                    }
                    break;
                case 1: // Desktop Acrylic Backdrop
                    App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
                    break;
                case 2: // No Transparency
                    App.MainWindow.SystemBackdrop = null;

                    App.MainWindow.SetTitleBarBackgroundColors(Application.Current.RequestedTheme == ApplicationTheme.Light ? Colors.White : Colors.Black);
                    break;
            }
            App.AppSettings.AppTransparencySetting = TransparencyDropdown.SelectedIndex;
        }

        private void UpdateAppTheme(object sender, SelectionChangedEventArgs e)
        {
            if (firstOpening) return;
            if (App.MainWindow.Content is not FrameworkElement rootElement) return;
            
            rootElement.RequestedTheme = (ElementTheme) ThemeDropdown.SelectedIndex;
            App.AppSettings.AppThemeSetting = ThemeDropdown.SelectedIndex;
        }

        private void UpdateRecordDelay(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (firstOpening) return;
            int newDelayTime = (int) RecordDelayBox.Value;
            if (newDelayTime <= 0) newDelayTime = 75;

            GlobalData.RecordStopDelay = newDelayTime;
            App.AppSettings.RecordEndWaitTime = newDelayTime;
        }

        private void UpdateToastStayTime(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (firstOpening) return;
            int newStayTime = (int) ToastDelayBox.Value;
            if (newStayTime <= 0) newStayTime = 1750;

            GlobalData.NotificationTimeout = newStayTime;
            App.AppSettings.NotificationTimeout = newStayTime;
        }

        private async void RefreshPitchData(object sender, RoutedEventArgs e)
        {
            // Actually, it just restarts the application.
            var confirmRefresh = new ContentDialog { Title = "Refresh Pitch Values?", Content = "Please save any unsaved work before refreshing", PrimaryButtonText = "Refresh", CloseButtonText = "Cancel", XamlRoot = base.Content.XamlRoot };
            var confirmResult = await confirmRefresh.ShowAsync();

            if (confirmResult == ContentDialogResult.Primary) Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
        }

        private void OpenPitchValuesFile(object sender, RoutedEventArgs e)
        {
            try { Process.Start(new ProcessStartInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Config\\defaultPitchData.json") { UseShellExecute = true }); } catch { return; }
        }

        private int BoolToInt(bool value)
        {
            return value == false ? 0 : 1;
        }

        private void OpenOutputFolder(object sender, RoutedEventArgs e)
        {
            string outFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Out";
            if (!Directory.Exists(outFolder)) Directory.CreateDirectory(outFolder); // The output folder could not exist if the user hasn't initialized a project for the first time
            Process outFolderOpenProcess = ShellCommandManager.CreateProcess("explorer", outFolder);
            outFolderOpenProcess.Start();
        }

        // Got lazy. It works though
        private async void ResetSettings(object sender, RoutedEventArgs e)
        {
            var confirmRefresh = new ContentDialog { Title = "Reset Settings?", Content = "Only your settings will be reverted to default values. App will restart", PrimaryButtonText = "Reset", CloseButtonText = "Cancel", XamlRoot = base.Content.XamlRoot };
            var result = await confirmRefresh.ShowAsync();
            if (result != ContentDialogResult.Primary) return;
            
            File.Delete($"{configFolder}\\AudioReplacer2-Config.json");
            Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
        }

        private async void ResetPitchData(object sender, RoutedEventArgs e)
        {
            var confirmRefresh = new ContentDialog { Title = "Reset Pitch Data?", Content = "Your custom pitch data will be reverted to default values. App will restart", PrimaryButtonText = "Reset", CloseButtonText = "Cancel", XamlRoot = base.Content.XamlRoot };
            var result = await confirmRefresh.ShowAsync();
            if (result != ContentDialogResult.Primary) return;
            
            File.Delete($"{configFolder}\\PitchData.json");
            Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
        }

        private async void ResetAll(object sender, RoutedEventArgs e)
        {
            var confirmRefresh = new ContentDialog { Title = "Reset Everything?", Content = "Both your settings and your custom pitch data will be reverted to original values. App will restart", PrimaryButtonText = "Reset", CloseButtonText = "Cancel", XamlRoot = base.Content.XamlRoot };
            var result = await confirmRefresh.ShowAsync();
            if (result != ContentDialogResult.Primary) return;
            
            File.Delete($"{configFolder}\\PitchData.json");
            File.Delete($"{configFolder}\\AudioReplacer2-Config.json");
            Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
        }
    }
}
