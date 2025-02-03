using AudioReplacer.Util;
using Microsoft.UI.Xaml;

namespace AudioReplacer.Windows.MainWindow.Pages;

public sealed partial class SettingsPage
{
    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    [Log]
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        App.DiscordController.SetDetails("In Settings Page");
        App.DiscordController.SetState("");
    }
}
