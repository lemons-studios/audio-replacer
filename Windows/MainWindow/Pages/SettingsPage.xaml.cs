using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AudioReplacer.Windows.MainWindow.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        App.DiscordController.SetDetails("In Settings Page");
        App.DiscordController.SetState("");
    }
}
