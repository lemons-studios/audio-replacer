using AudioReplacer.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using TitleBarDrag;

namespace AudioReplacer
{
    public sealed partial class MainWindow
    {
        private readonly Dictionary<Type, Page> pageCache = new();

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
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
        }

        public void FolderPickerHelper()
        {
            var switchType = typeof(RecordPage);
            if (!pageCache.TryGetValue(switchType, out var page))
            {
                page = (Page) Activator.CreateInstance(switchType);
                pageCache[switchType] = page;
            }
            MainFrame.Content = page;
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
                page = (Page) Activator.CreateInstance(pageSwitchType);
                pageCache[pageSwitchType] = page;
            }
            MainFrame.Content = page;
        }
    }
}