using AudioReplacer.MainWindow.Pages;
using AudioReplacer.MainWindow.Util;
using CommunityToolkit.WinUI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Velopack;
// ReSharper disable RedundantJumpStatement

namespace AudioReplacer.MainWindow;

public sealed partial class MainWindow
{
    private readonly Dictionary<Type, Page> pageCache = new();
    
    public MainWindow()
    {
        InitializeComponent();
        App.AppWindow = App.GetAppWindowForCurrentWindow(this);
        App.AppWindow.Closing += OnClose;
        ExtendsContentIntoTitleBar = true;
        AppTitle.Text = $"Audio Replacer {AppFunctions.GetAppVersion()}";
        SetTitleBar(AppTitleBar);

        SystemBackdrop = new MicaBackdrop();
        MainFrame.Navigate(typeof(HomePage));

        const string url = "https://f004.backblazeb2.com/file/audio-replacer-updates/";
        AppUpdater.AppUpdateManager = new UpdateManager(url);
        AppUpdater.OnUpdateFound += OnUpdateFound;
        Task.Run(AppUpdater.SearchForUpdates);

        App.AppWindow.SetIcon(@"Assets\AppIcon.ico");
    }

    private void OnUpdateFound()
    {
        UpdateAvailableNotification.DispatcherQueue.TryEnqueue(() =>
        {
            UpdateAvailableNotification.IsOpen = true;
        });
    }

    [Log]
    public async Task ShowNotification(InfoBarSeverity severity, string title, string message, bool autoclose = false, bool closable = true, bool replaceExistingNotifications = true)
    {
        if (!replaceExistingNotifications && GeneralNotificationPopup.IsOpen)
            return;
        
        try
        {
            if (replaceExistingNotifications)
            {
                await GeneralNotificationPopup.DispatcherQueue.EnqueueAsync(() =>
                {
                    GeneralNotificationPopup.IsOpen = false;
                });
                await InProgressNotification.DispatcherQueue.EnqueueAsync(() =>
                {
                    InProgressNotification.IsOpen = false;
                });
                await CompletionNotification.DispatcherQueue.EnqueueAsync(() =>
                {
                    CompletionNotification.IsOpen = false;
                });
            }

            await GeneralNotificationPopup.DispatcherQueue.EnqueueAsync(() =>
            {
                GeneralNotificationPopup.Severity = severity;
                GeneralNotificationPopup.Title = title;
                GeneralNotificationPopup.Message = message;
                GeneralNotificationPopup.IsClosable = closable;
                GeneralNotificationPopup.IsOpen = true;
            });

            if (autoclose)
            {
                await Task.Delay(App.AppSettings.NotificationTimeout);
                try
                {
                    await GeneralNotificationPopup.DispatcherQueue.EnqueueAsync(() =>
                    {
                        GeneralNotificationPopup.IsOpen = false;
                    });
                }
                catch
                {
                    return;
                }
            }
        }
        catch (NullReferenceException)
        {
            return;
        }
    }

    [Log]
    public void ShowProgressNotification(string title, string message)
    {
        InProgressNotification.DispatcherQueue.TryEnqueue(() =>
        {
            InProgressNotification.Title = title;
            InProgressNotification.Message = message;

            InProgressNotification.IsOpen = true;
        });
    }

    public void HideProgressNotification()
    {
        InProgressNotification.DispatcherQueue.TryEnqueue(() =>
        {
            InProgressNotification.IsOpen = false;
        });
    }

    public bool IsProgressNotificationOpen()
    {
        return InProgressNotification.IsOpen;
    }

    public void SetProgressMessage(string message)
    {
        InProgressNotification.DispatcherQueue.TryEnqueue(() =>
        {
            InProgressNotification.Message = message;
        });
    }

    public void ShowCompletionNotification(string title, string message, float percentage = 0f)
    {
        CompletionNotification.DispatcherQueue.TryEnqueue(() =>
        {
            CompletionNotification.Title = title;
            CompletionNotification.Message = message;

            CompletionNotification.IsOpen = !CompletionNotification.IsOpen;
        });
        CompletionProgressBar.DispatcherQueue.TryEnqueue(() =>
        {
            CompletionProgressBar.Value = percentage;
        });
    }

    public void HideCompletionNotification()
    {
        CompletionNotification.DispatcherQueue.TryEnqueue(() =>
        {
            CompletionNotification.IsOpen = false;
        });
    }

    public bool IsCompletionNotificationOpen()
    {
        return CompletionNotification.IsOpen;
    }

    public void SetCompletionMessage(string message, float percentage)
    {
        CompletionNotification.DispatcherQueue.TryEnqueue(() =>
        {
            CompletionNotification.Message = message;
        });
        CompletionProgressBar.DispatcherQueue.TryEnqueue(() =>
        {
            CompletionProgressBar.Value = percentage;
        });
    }

    [Log]
    private void Navigate(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var pageSwitchType = typeof(HomePage); // Default page
        if (args.InvokedItemContainer != null && args.InvokedItemContainer.Tag is string tag)
        {
            pageSwitchType = tag switch
            {
                "Record" => typeof(RecordPage),
                "Settings" => typeof(SettingsPage),
                "Data Editor" => typeof(DataEditor),
                "Home" => typeof(HomePage),
                _ => pageSwitchType
            };
        }

        if (!pageCache.TryGetValue(pageSwitchType, out var page))
        {
            page = (Page) Activator.CreateInstance(pageSwitchType);
            pageCache[pageSwitchType] = page;
        }
        MainFrame.Content = page;
    }

    public void OpenRecordPage()
    {
        var pageType = typeof(RecordPage);
        if (!pageCache.TryGetValue(pageType, out var page))
        {
            page = (Page) Activator.CreateInstance(pageType);
            pageCache[pageType] = page;
        }

        MainFrame.Content = page;
    }

    [Log]
    private void OnClose(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (AppProperties.InRecordState)
        {
            File.Delete(ProjectFileUtils.GetOutFilePath());
        }
    }

    private async void DownloadUpdate(object sender, RoutedEventArgs e)
    {
        UpdateAvailableNotification.DispatcherQueue.TryEnqueue(() =>
        {
            UpdateAvailableNotification.IsOpen = false;
        });

        ShowProgressNotification("Downloading Updates", "App will restart once updates are downloaded");
        await AppUpdater.UpdateApplication();
    }
}