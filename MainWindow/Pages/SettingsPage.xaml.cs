namespace AudioReplacer.MainWindow.Pages;

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
        App.DiscordController.SetDetails("Settings Page");
        App.DiscordController.SetState("");
    }
}
