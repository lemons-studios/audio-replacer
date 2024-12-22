using System;
using System.Diagnostics;
using System.IO;
using AudioReplacer.Util;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace AudioReplacer.Pages
{
    public sealed partial class SettingsPage
    {
        private readonly bool firstOpening;
        private readonly string configFolder = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\audio-replacer\config";

        public SettingsPage()
        {
            InitializeComponent();
            // Initialize all the data
            ThemeDropdown.SelectedIndex = App.AppSettings.AppThemeSetting;
            TransparencyDropdown.SelectedIndex = App.AppSettings.AppTransparencySetting;
            UpdateCheckSwitch.IsOn = GlobalData.UpdateChecksAllowed;
            ProjectMemorySwitch.IsOn = App.AppSettings.RememberSelectedFolder == 1;
            RandomizeInputSwitch.IsOn = GlobalData.InputRandomizationEnabled;
            FanfareToggle.IsOn = GlobalData.EnableFanfare;
            ToastDelayBox.Value = GlobalData.NotificationTimeout;
            StopDelayBox.Value = GlobalData.RecordStopDelay;
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
                    if (MicaController.IsSupported()) App.MainWindow.SystemBackdrop = new MicaBackdrop();
                    else
                    {
                        App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
                        TransparencyDropdown.SelectedIndex = 1; // If mica isn't supported, switch to acrylic
                    }
                    break;
                case 1: // Desktop Acrylic Backdrop
                    App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
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

        private void UpdateDelayTimes(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (firstOpening) return;
            switch (sender == StopDelayBox)
            {
                case true:
                    int newDelayTime = (int) MathF.Max((float) StopDelayBox.Value, 0);
                    GlobalData.RecordStopDelay = newDelayTime;
                    App.AppSettings.RecordEndWaitTime = newDelayTime;
                    break;
                case false:
                    int newStayTime = (int) MathF.Max((float) ToastDelayBox.Value, 500);
                    GlobalData.NotificationTimeout = newStayTime;
                    App.AppSettings.NotificationTimeout = newStayTime;
                    break;
            }
        }

        private async void RefreshPitchData(object sender, RoutedEventArgs e)
        {
            // Actually, it just restarts the application.
            var confirmRefresh = new ContentDialog { Title = "Refresh Pitch Values?", Content = "Please save any unsaved work before refreshing", PrimaryButtonText = "Refresh", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
            var confirmResult = await confirmRefresh.ShowAsync();
            if (confirmResult == ContentDialogResult.Primary) Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
        }

        private void OpenOutputFolder(object sender, RoutedEventArgs e)
        {
            string outFolder = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\audio-replacer\out";
            if (!Directory.Exists(outFolder)) Directory.CreateDirectory(outFolder); // The output folder could not exist if the user hasn't initialized a project for the first time
            Process outFolderOpenProcess = ShellCommandManager.CreateProcess("explorer", outFolder);
            outFolderOpenProcess.Start();
        }

        private async void ResetCustomData(object sender, RoutedEventArgs e)
        {
            if ((SettingsCard) sender == AllDataOption)
            {
                var confirmRefresh = new ContentDialog { Title = "Reset Everything?", Content = "App will restart", PrimaryButtonText = "Reset Data", SecondaryButtonText = "Reset Everything", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot, Width = 500 };
                var result = await confirmRefresh.ShowAsync();

                File.Delete($@"{configFolder}\PitchData.json");
                File.Delete($@"{configFolder}\EffectsData.json");
                if (result == ContentDialogResult.Secondary) File.Delete($@"{configFolder}\AppSettings.json");
            }
            else if ((SettingsCard) sender == SettingsOption)
            {
                var confirmRefresh = new ContentDialog { Title = "Reset Settings?", Content = "Only your settings will be reverted to default values. App will restart", PrimaryButtonText = "Reset", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
                var result = await confirmRefresh.ShowAsync();
                if (result != ContentDialogResult.Primary) return;
                File.Delete($@"{configFolder}\AppSettings.json");
            }
            else
            {
                string source = (SettingsCard) sender == PitchData ? "pitch" : "effect";
                var confirmReset = new ContentDialog { Title = $"Reset {source} Data?", Content = $"Your custom {source} data will be reverted to default values. App will restart", PrimaryButtonText = "Reset", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
                var result = await confirmReset.ShowAsync();
                if (result != ContentDialogResult.Primary) return;

                string file = source.Equals("pitch") ? "PitchData.json" : "EffectData.json";
                File.Delete(Path.Combine(configFolder, file));
            }
            Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
        }

        private async void OpenCustomDataFile(object sender, RoutedEventArgs e)
        {
            var fileOption = new ContentDialog { Title = "Pick File", Content = "Which custom data file would you like to open?", PrimaryButtonText = "Pitch Data", SecondaryButtonText = "Effects Data", CloseButtonText = "Cancel" ,XamlRoot = Content.XamlRoot };
            var result = await fileOption.ShowAsync();
            if (result == ContentDialogResult.None) return;
            try
            {
                string file = result == ContentDialogResult.Secondary ? Path.Combine(configFolder, "EffectsData.json") : Path.Combine(configFolder, "PitchData.json");
                var fileOpenProcess = ShellCommandManager.CreateProcess("cmd", $"/c start {file}", true, false, false, true);
                fileOpenProcess.Start();
            }
            catch { return; }
        }

        private void ToggleFileRandomization(object sender, RoutedEventArgs e)
        {
            GlobalData.InputRandomizationEnabled = RandomizeInputSwitch.IsOn;
            App.AppSettings.InputRandomizationEnabled = BoolToInt(RandomizeInputSwitch.IsOn);
        }

        private void ToggleFolderMemory(object sender, RoutedEventArgs e)
        {
            App.AppSettings.RememberSelectedFolder = BoolToInt(ProjectMemorySwitch.IsOn);
        }

        private void ToggleAdditionalDetails(object sender, RoutedEventArgs e)
        {
            GlobalData.ShowAudioEffectDetails = ShowAdditionalDetailsSwitch.IsOn;
            App.AppSettings.ShowEffectSelection = BoolToInt(ShowAdditionalDetailsSwitch.IsOn);
        }

        private void ToggleFanfare(object sender, RoutedEventArgs e)
        {
            GlobalData.EnableFanfare = FanfareToggle.IsOn;
            App.AppSettings.EnableFanfare = BoolToInt(FanfareToggle.IsOn);
        }

        private int BoolToInt(bool value) { return value == false ? 0 : 1; }

        private void UpdateStartDelay(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            int newDelayTime = (int) MathF.Max((float) StartDelayBox.Value, 0);
            GlobalData.RecordStartDelay = newDelayTime;
            App.AppSettings.RecordStartWaitTime = newDelayTime;
        }
    }
}
