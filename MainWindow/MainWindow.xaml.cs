using AudioReplacer.Generic;
using AudioReplacer.MainWindow.Pages;
using AudioReplacer.MainWindow.Util;
using AudioReplacer.Util;
using CommunityToolkit.WinUI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TitleBarDrag;
using Velopack;
using Windows.Storage.Pickers;
using WinRT.Interop;

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
        MainFrame.Navigate(typeof(RecordPage));

        // Remove the change folder button from the draggable region on the titlebar
        // The official Microsoft-intended method is absolutely wild, so I'll use this really obscure nuget package
        // Sure, the package may only have 160ish downloads, but it abstracts the Microsoft way of making the code very nicely
        var dragRegions = new DragRegions(this, AppTitleBar)
        {
            NonDragElements = [FolderChanger]
        };

        var lastSelectedFolder = App.AppSettings.LastSelectedFolder;
        if (App.AppSettings.RememberSelectedFolder == 1 && lastSelectedFolder != string.Empty)
        {
            ProjectFileUtils.SetProjectData(App.AppSettings.LastSelectedFolder);
        }
        var url = "https://f004.backblazeb2.com/file/audio-replacer-updates/";
        AppUpdater.AppUpdateManager = new UpdateManager(url);
        Task.Run(AppUpdater.UpdateApplication);
        AppUpdater.OnUpdateFound += OnUpdateFound;
    }

    private void OnUpdateFound()
    {
        ToggleProgressNotification("Updates found", "App will restart once updates are downloaded");
    }

    [Log]
    public async Task ShowNotification(InfoBarSeverity severity, string title, string message, bool autoclose = false, bool closable = true, bool replaceExistingNotifications = true)
    {
        if (!replaceExistingNotifications && GeneralNotificationPopup.IsOpen)
        {
            return;
        }
        else
        {
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
            catch (NullReferenceException e)
            {
                return;
            }
        }
    }

    [Log]
    public void ToggleProgressNotification(string title, string message)
    {
        InProgressNotification.DispatcherQueue.TryEnqueue(() =>
        {
            InProgressNotification.Title = title;
            InProgressNotification.Message = message;

            InProgressNotification.IsOpen = !InProgressNotification.IsOpen;
        });
    }

    // For a future part of the app
    public void SetProgressMessage(string message)
    {
        InProgressNotification.DispatcherQueue.TryEnqueue(() =>
        {
            InProgressNotification.Message = message;
        });
    }

    [Log]
    private async void ChangeProjectFolder(object sender, RoutedEventArgs e)
    {
        var folderPicker = new FolderPicker { FileTypeFilter = { "*" } };
        InitializeWithWindow.Initialize(folderPicker, WindowNative.GetWindowHandle(this));
        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null && !string.IsNullOrEmpty(folder.Path))
        {
            string folderPath = folder.Path;
            if (AppFunctions.IntToBool(App.AppSettings.RememberSelectedFolder))
                App.AppSettings.LastSelectedFolder = folderPath;
            ProjectFileUtils.SetProjectData(folderPath);
        }
    }

    [Log]
    private void Navigate(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var pageSwitchType = typeof(RecordPage); // Default page of project
        if (args.InvokedItemContainer != null && args.InvokedItemContainer.Tag is string tag)
        {
            pageSwitchType = tag switch
            {
                "Record" => typeof(RecordPage),
                "Settings" => typeof(SettingsPage),
                "Data Editor" => typeof(DataEditor),
                "What's New" => typeof(ReleaseLogsPage),
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

    [Log]
    private void OnClose(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (AppProperties.InRecordState)
        {
            File.Delete(ProjectFileUtils.GetOutFilePath());
        }
    }
}
