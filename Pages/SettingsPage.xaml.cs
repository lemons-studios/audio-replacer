using AudioReplacer2.Util;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace AudioReplacer2.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void ToggleUpdateChecks(object sender, RoutedEventArgs e)
        {
            GlobalData.updateChecksAllowed = UpdateCheckSwitch.IsOn;
        }

        private void ToggleTransparencyMode(object sender, SelectionChangedEventArgs e)
        {
            switch (TransparencyDropdown.SelectedIndex)
            {
                case 0:
                    if(MicaController.IsSupported()) App.MainWindow.SystemBackdrop = new MicaBackdrop();
                    else
                    {
                        App.systemAppTheme = ApplicationTheme.Light;
                        App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
                        TransparencyDropdown.SelectedIndex = 1;
                    }
                    break;
                case 1:
                    App.MainWindow.SystemBackdrop = new DesktopAcrylicBackdrop();
                    break;
            }
        }

        private void UpdateAppTheme(object sender, SelectionChangedEventArgs e)
        {
            if (App.MainWindow.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = (ElementTheme) ThemeDropdown.SelectedIndex;
            }
        }
    }
}
