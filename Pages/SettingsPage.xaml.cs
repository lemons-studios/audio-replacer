using AudioReplacer.Generic;
using AudioReplacer.Util;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Navigation;

namespace AudioReplacer.Pages
{
    public sealed partial class SettingsPage
    {
        private readonly string configFolder = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\audio-replacer\config";

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ThemeDropdown.SelectedIndex = App.AppSettings.AppThemeSetting;
            TransparencyDropdown.SelectedIndex = App.AppSettings.AppTransparencySetting;
            UpdateCheckSwitch.IsOn = AppGeneric.UpdateChecksAllowed;
            ProjectMemorySwitch.IsOn = App.AppSettings.RememberSelectedFolder == 1;
            RandomizeInputSwitch.IsOn = AppGeneric.InputRandomizationEnabled;
            ToastDelayBox.Value = AppGeneric.NotificationTimeout;
            StopDelayBox.Value = AppGeneric.RecordStopDelay;

            App.DiscordController.SetDetails("Changing Settings");
        }

        private void ToggleUpdateChecks(object sender, RoutedEventArgs e)
        {
            AppGeneric.UpdateChecksAllowed = UpdateCheckSwitch.IsOn;
            App.AppSettings.AppUpdateCheck = AppGeneric.BoolToInt(UpdateCheckSwitch.IsOn);
        }

        private void ToggleTransparencyMode(object sender, SelectionChangedEventArgs e)
        {
            if (TransparencyDropdown.SelectedIndex == 0 && MicaController.IsSupported())
            {
                App.MainWindow.SystemBackdrop = new MicaBackdrop();
            }
            else
            {
                // If Mica isn't supported, switch to Acrylic
                App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
                TransparencyDropdown.SelectedIndex = 1; 
            }
            App.AppSettings.AppTransparencySetting = TransparencyDropdown.SelectedIndex;
        }

        private void UpdateAppTheme(object sender, SelectionChangedEventArgs e)
        {
            if (App.MainWindow.Content is not FrameworkElement rootElement) return;
            rootElement.RequestedTheme = (ElementTheme) ThemeDropdown.SelectedIndex;
            App.AppSettings.AppThemeSetting = ThemeDropdown.SelectedIndex;
        }

        private void UpdateRecordStopDelay(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            var newDelayTime = (int) Math.Max(StopDelayBox.Value, 0);
            AppGeneric.RecordStopDelay = newDelayTime;
            App.AppSettings.RecordEndWaitTime = newDelayTime;
        }

        private void UpdateStartDelay(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            var newDelayTime = (int) Math.Max(StartDelayBox.Value, 0);
            AppGeneric.RecordStartDelay = newDelayTime;
            App.AppSettings.RecordStartWaitTime = newDelayTime;
        }

        private void UpdateNotificationDelay(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            var newTime = (int) Math.Max(ToastDelayBox.Value, 500);
            AppGeneric.NotificationTimeout = newTime;
            App.AppSettings.NotificationTimeout = newTime;
        }

        private async void RefreshPitchData(object sender, RoutedEventArgs e)
        {
            // Actually, it just restarts the application.
            var confirmRefresh = new ContentDialog { Title = "Refresh Pitch Values?", Content = "Please save any unsaved work before refreshing", PrimaryButtonText = "Refresh", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
            var result = await confirmRefresh.ShowAsync();

            if (result == ContentDialogResult.Primary) 
                AppGeneric.RestartApp();
        }

        private void OpenOutputFolder(object sender, RoutedEventArgs e)
        {
            var outFolder = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\audio-replacer\out";
            if (!Directory.Exists(outFolder)) 
                Directory.CreateDirectory(outFolder); // The output folder may not exist if the user hasn't initialized a project for the first time

            Task.Run(() => AppGeneric.SpawnProcess("explorer", outFolder));
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
            AppGeneric.RestartApp();
        }

        private async void OpenCustomDataFile(object sender, RoutedEventArgs e)
        {
            var fileOption = new ContentDialog { Title = "Pick File", Content = "Which custom data file would you like to open?", PrimaryButtonText = "Pitch Data", SecondaryButtonText = "Effects Data", CloseButtonText = "Cancel", XamlRoot = Content.XamlRoot };
            var result = await fileOption.ShowAsync();
            if (result == ContentDialogResult.None) return;
            try
            {
                var file = result == ContentDialogResult.Secondary
                    ? Path.Combine(configFolder, "EffectsData.json")
                    : Path.Combine(configFolder, "PitchData.json");

                await AppGeneric.SpawnProcess("cmd", $"/c start {file}");
            }
            catch
            {
                // No need to handle exception
            }
        }

        private void ToggleFileRandomization(object sender, RoutedEventArgs e)
        {
            AppGeneric.InputRandomizationEnabled = RandomizeInputSwitch.IsOn;
            App.AppSettings.InputRandomizationEnabled = AppGeneric.BoolToInt(RandomizeInputSwitch.IsOn);
        }

        private void ToggleFolderMemory(object sender, RoutedEventArgs e)
        {
            App.AppSettings.RememberSelectedFolder = AppGeneric.BoolToInt(ProjectMemorySwitch.IsOn);
        }

        private void ToggleAdditionalDetails(object sender, RoutedEventArgs e)
        {
            AppGeneric.ShowAudioEffectDetails = ShowAdditionalDetailsSwitch.IsOn;
            App.AppSettings.ShowEffectSelection = AppGeneric.BoolToInt(ShowAdditionalDetailsSwitch.IsOn);
        }
    }
}
