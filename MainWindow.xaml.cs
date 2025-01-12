using AudioReplacer.Util;
using AudioReplacer.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using TitleBarDrag;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AudioReplacer
{
    public sealed partial class MainWindow
    {
        private readonly Dictionary<Type, Page> pageCache = new();

        public MainWindow()
        {
            InitializeComponent();
            App.AppWindow = App.GetAppWindowForCurrentWindow(this);
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            SystemBackdrop = new MicaBackdrop();
            MainFrame.Navigate(typeof(RecordPage));

            // Remove the change folder button from the draggable region on the titlebar
            // The official Microsoft-intended method is absolutely wild, so I'll use this really obscure nuget package
            // Sure, the package may only have 160ish downloads, but it abstracts the Microsoft way of making the code very nicely
            var dragRegions = new DragRegions(this, AppTitleBar)
            {
                NonDragElements = [ FolderChanger ]
            };

            string lastSelectedFolder = App.AppSettings.LastSelectedFolder;
            if (App.AppSettings.RememberSelectedFolder == 1 && lastSelectedFolder != string.Empty)
            {
                ProjectFileUtils.SetProjectData(App.AppSettings.LastSelectedFolder);
            }
        }

        private async void ChangeProjectFolder(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker { FileTypeFilter = { "*" } };
            InitializeWithWindow.Initialize(folderPicker, WindowNative.GetWindowHandle(this));
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null && !string.IsNullOrEmpty(folder.Path))
            {
                string folderPath = folder.Path;
                if (Generic.IntToBool(App.AppSettings.RememberSelectedFolder))
                    App.AppSettings.LastSelectedFolder = folderPath;
                ProjectFileUtils.SetProjectData(folderPath);
            }
        }

        public void DisableFolderChanger()
        {
            FolderChanger.IsEnabled = false;
        }

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
                    "Update Logs" => typeof(ReleaseLogsPage),
                    _ => pageSwitchType
                };
            }

            if (!pageCache.TryGetValue(pageSwitchType, out var page))
            {
                page = (Page)Activator.CreateInstance(pageSwitchType);
                pageCache[pageSwitchType] = page;
            }
            MainFrame.Content = page;
        }
    }
}