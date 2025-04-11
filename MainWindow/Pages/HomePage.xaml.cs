using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace AudioReplacer.MainWindow.Pages;

public sealed partial class HomePage
{
    public HomePage()
    {
        InitializeComponent();
        MarkdownText.Config = new MarkdownConfig();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs a)
    {
        App.DiscordController.SetDetails("Home Page");
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
