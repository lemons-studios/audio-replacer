using System.IO;
using AudioReplacer2.Util;
using AudioReplacer2.Pages;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;
using WinUIEx;
using Microsoft.UI;

namespace AudioReplacer2
{
    public sealed partial class MainWindow : WindowEx
    {
        // Needed for button state switching. These are in MainWindow because checking for them is a part of the window shutting down (Okay this probably isn't the best way to do this but if it ain't broke, don't fix it)
        public static bool isProcessing;
        public static bool isRecording;
        public static bool projectInitialized;

        public static string currentFile;

        public MainWindow()
        {
            InitializeComponent();
            GlobalData.appWindow = GetAppWindowForCurrentWindow(this);
            GlobalData.appWindow.Closing += OnWindowClose;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            AppTitle.Text = $"Audio Replacer {GlobalData.GetAppVersion()}";

            // Enable mica if it's supported. If not, fall back to Acrylic. Mica is not supported on any version of Windows 10
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

            // Finally, open the recording page
            ContentFrame.Navigate(typeof(RecordPage));
        }

        // Thanks StackOverflow man!
        private AppWindow GetAppWindowForCurrentWindow(object window)
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);

            return AppWindow.GetFromWindowId(myWndId);
        }

        private void OnWindowClose(object sender, AppWindowClosingEventArgs args) { if (MainWindow.projectInitialized && (MainWindow.isProcessing || MainWindow.isRecording)) File.Delete(MainWindow.currentFile); }
    }
}
