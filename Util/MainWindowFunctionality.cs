using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using Windows.Media.Core;
using Windows.Media.Playback;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;

namespace AudioReplacer2.Util
{
    // Easily removes a good chunk of code from MainWindow.xaml.cs. much easier to navigate through everything now!
    public class MainWindowFunctionality
    {
        public bool ToBool(int value)
        {
            return value > 0;
        }

        public string BoolToYesNo(bool value)
        {
            return value ? "Yes" : "No";
        }

        public Visibility ToVisibility(bool x)
        {
            return x ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ToggleButton(Button button, bool toggle)
        {
            Visibility toggleVisibility = ToVisibility(toggle);

            button.IsEnabled = toggle;
            button.Visibility = toggleVisibility;
        }

        public MediaSource MediaSourceFromURI(string path)
        {
            return MediaSource.CreateFromUri(new Uri(path));
        }

        // Thanks StackOverflow man!
        public AppWindow GetAppWindowForCurrentWindow(object window)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }

        public float GetPitchModifier(int index, string pitchData)
        {
            // TODO: JSON array is more evil than the evil switch case, replace with 2d array containing information on both character and pitch modification for (somewhat) easy edit-ability by any end user
            JArray jPitchData = JArray.Parse(pitchData);
            try
            {
                return (float)jPitchData[index]["pitchModification"];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
        }
    }
}
