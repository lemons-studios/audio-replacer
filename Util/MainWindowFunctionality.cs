using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using Windows.Media.Core;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace AudioReplacer2.Util
{
    // Easily removes a good chunk of code from MainWindow.xaml.cs. much easier to navigate through everything now!
    public class MainWindowFunctionality
    {
        private readonly List<string> pitchMenuTitles;
        private readonly List<float> pitchValues;

        public MainWindowFunctionality(ComboBox pitchComboBox)
        {
            pitchMenuTitles = new List<string>();
            pitchValues = new List<float>();

            for (int i = 0; i < PitchData.pitchData.Length; i++)
            {
                pitchValues.Add(ParseFloat(PitchData.pitchData[i][0])); // Element 0 should always be a pitch value
                pitchMenuTitles.Add(PitchData.pitchData[i][1]); // Element 1 should always be a name for the combo box
            }
        }

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

        public List<string> GetPitchTitles()
        {
            return pitchMenuTitles;
        }

        public float GetPitchModifier(int index)
        {
            try { return pitchValues[index]; } catch { return 1; }
        }

        public string GetAppVersion()
        {
            var appVersion = Package.Current.Id.Version;
            int major = appVersion.Major;
            int minor = appVersion.Minor;
            int build = appVersion.Build;

            return build != 0 ? $"{major}.{minor}.{build}" : $"{major}.{minor}";
        }

        private float ParseFloat(string value)
        {
            try { return float.Parse(value); } catch { return 1; }
        }
    }
}
