using AudioReplacer.Generic;
using AudioReplacer.Pages;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using WinRT.Interop;

namespace AudioReplacer
{
    public sealed partial class MainWindow
    {
        private readonly Dictionary<Type, Page> pageCache = new();

        // Needed for button state switching. These are in MainWindow because checking for them is a part of the window shutting down (Okay this probably isn't the best way to do this but if it ain't broke, don't fix it)
        public static bool IsProcessing;
        public static bool IsRecording;
        public static bool ProjectInitialized;
        public static string CurrentFile;

        public MainWindow()
        {
            InitializeComponent();
            AppGeneric.AppWindow = GetAppWindowForCurrentWindow(this);
            AppGeneric.AppWindow.Closing += OnWindowClose;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            AppTitleText.Text = $"Audio Replacer {AppGeneric.GetAppVersion()}";

            // Set everything that can be set by the settings.
            if (Content is FrameworkElement rootElement) rootElement.RequestedTheme = (ElementTheme) App.AppSettings.AppThemeSetting;
            switch (App.AppSettings.AppTransparencySetting)
            {
                case 0:
                    switch (MicaController.IsSupported())
                    {
                        case true:
                            var micaBackdrop = new MicaBackdrop();
                            SystemBackdrop = micaBackdrop;
                            break;
                        case false:
                            var acrylicBackdrop = new DesktopAcrylicBackdrop();
                            SystemBackdrop = acrylicBackdrop;
                            break;
                    }
                    break;
                case 1:
                    {
                        var acrylicBackdrop = new DesktopAcrylicBackdrop();
                        SystemBackdrop = acrylicBackdrop;
                        break;
                    }
                default:
                    SystemBackdrop = null;
                    break;
            }

            // Open the recording page (Stole this from the method below)
            if (!pageCache.TryGetValue(typeof(RecordPage), out var page))
            {
                page = (Page) Activator.CreateInstance(typeof(RecordPage));
                pageCache[typeof(RecordPage)] = page;
            }

            ContentFrame.Content = page;
        }

        private AppWindow GetAppWindowForCurrentWindow(object window) // Thanks StackOverflow man!
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            var currentWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(currentWndId);
        }

        public void DisableFolderChanger()
        {
            ProjectFolderButton.IsEnabled = false;
        }

        private void SwitchPageContent(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var pageSwitchType = typeof(RecordPage); // Default page of project
            if (args.InvokedItemContainer != null && args.InvokedItemContainer.Tag is string tag)
            {
                pageSwitchType = tag switch
                {
                    "Record" => typeof(RecordPage),
                    "Settings" => typeof(SettingsPage),
                    "Data Editor" => typeof(DataEditor),
                    _ => pageSwitchType
                };
            }

            ProjectFolderButton.Visibility = args.InvokedItemContainer!.Tag.Equals("Record")
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (!pageCache.TryGetValue(pageSwitchType, out var page))
            {
                page = (Page) Activator.CreateInstance(pageSwitchType);
                pageCache[pageSwitchType] = page;
            }
            ContentFrame.Content = page;
        }

        private void OnWindowClose(object sender, AppWindowClosingEventArgs args)
        {
            if (ProjectInitialized && (IsProcessing || IsRecording)) File.Delete(CurrentFile);
            App.DiscordController.DisposeRpc();
        }

        private void ChangeProjectFolder(object sender, RoutedEventArgs e)
        {
            var currentPage = ContentFrame.Content as RecordPage;
            if (currentPage != null) currentPage.SelectProjectFolder(sender, e);
        }
    }
}
