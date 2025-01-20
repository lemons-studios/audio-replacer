using System.Threading.Tasks;
using AudioReplacer.Util;
using Markdig;
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
        Task.Run(ConvertMarkdown);
    }

    private async Task ConvertMarkdown()
    {
        string markdown = await Generic.GetDataFromGithub("body");
        MarkdownText.Text = markdown;
    }
}
