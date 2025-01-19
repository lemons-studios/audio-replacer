using Microsoft.UI.Xaml;
namespace AudioReplacer.Windows.MainWindow.Pages;

public sealed partial class ReleaseLogsPage
{
    public ReleaseLogsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        App.DiscordController.SetDetails("Viewing Release Logs");
        App.DiscordController.SetState("");
    }
}
