using AudioReplacer2.Util;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace AudioReplacer2.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private bool firstOpening = true;
        public SettingsPage()
        {
            InitializeComponent();
            // Initialize all the data
            ThemeDropdown.SelectedItem = App.AppSettings.AppThemeSetting;
            TransparencyDropdown.SelectedItem = App.AppSettings.AppTransparencySetting;
            UpdateCheckSwitch.IsOn = GlobalData.updateChecksAllowed;
            ToastDelayBox.Value = GlobalData.notificationTimeout;
            RecordDelayBox.Value = GlobalData.recordStopDelay;
            firstOpening = false;
        }

        private void ToggleUpdateChecks(object sender, RoutedEventArgs e)
        {
            if (firstOpening) return;
            GlobalData.updateChecksAllowed = UpdateCheckSwitch.IsOn;
            App.AppSettings.AppUpdateCheck = BoolToInt(UpdateCheckSwitch.IsOn);
        }

        private void ToggleTransparencyMode(object sender, SelectionChangedEventArgs e)
        {
            if (firstOpening) return;
            switch (TransparencyDropdown.SelectedIndex)
            {
                case 0:
                    if(MicaController.IsSupported()) App.MainWindow.SystemBackdrop = new MicaBackdrop();
                    else
                    {
                        App.SystemAppTheme = ApplicationTheme.Light;
                        App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
                        TransparencyDropdown.SelectedIndex = 1;
                    }
                    break;
                case 1:
                    App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
                    break;
                case 2:
                    App.MainWindow.SystemBackdrop = null;
                    break;
            }

            App.AppSettings.AppTransparencySetting = TransparencyDropdown.SelectedIndex;
        }

        private void UpdateAppTheme(object sender, SelectionChangedEventArgs e)
        {
            if (firstOpening) return;
            if (App.MainWindow.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = (ElementTheme) ThemeDropdown.SelectedIndex;
                App.AppSettings.AppThemeSetting = ThemeDropdown.SelectedIndex;
            }
        }

        private void UpdateRecordDelay(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (firstOpening) return;
            int newDelayTime = (int) RecordDelayBox.Value;
            if (newDelayTime <= 0) newDelayTime = 75;

            GlobalData.recordStopDelay = newDelayTime;
            App.AppSettings.RecordEndWaitTime = newDelayTime;
        }

        private void UpdateToastStayTime(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (firstOpening) return;
            int newStayTime = (int) ToastDelayBox.Value;
            if (newStayTime <= 0) newStayTime = 1750;

            GlobalData.notificationTimeout = newStayTime;
            App.AppSettings.NotificationTimeout = newStayTime;
        }

        private int BoolToInt(bool value)
        {
            return value == false ? 0 : 1;
        }
    }
}
