using AudioReplacer.Generic;
using AudioReplacer.Util;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace AudioReplacer.Windows.MainWindow.Pages;

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
        var markdown = await AppFunctions.GetDataFromGithub("body");

        await MarkdownText.DispatcherQueue.EnqueueAsync(() =>
        {
            MarkdownText.Text = markdown;
        });
    }
}
