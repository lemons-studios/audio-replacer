using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.WinUI;

namespace AudioReplacer.MainWindow.Pages;

public sealed partial class ReleaseLogsPage
{
    public ReleaseLogsPage()
    {
        InitializeComponent();
        MarkdownText.Config = new MarkdownConfig();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        App.DiscordController.SetDetails("Viewing Release Logs");
        App.DiscordController.SetState("");
        Task.Run(SetContent);
    }

    [Log]
    private async Task SetContent()
    {
        var markdown = await AppFunctions.GetJsonFromUrl("https://api.github.com/repos/lemons-studios/audio-replacer/releases/latest", "body");

        await MarkdownText.DispatcherQueue.EnqueueAsync(() =>
        {
            MarkdownText.Text = markdown;
        });
    }
}
