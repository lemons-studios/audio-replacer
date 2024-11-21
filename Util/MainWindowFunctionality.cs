using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
        private readonly WebRequest webRequest;
        private string webVersion;

        public MainWindowFunctionality(ComboBox pitchComboBox)
        {
            webRequest = new WebRequest();
            pitchMenuTitles = [];
            pitchValues = [];

            foreach (var data in PitchData.pitchData)
            {
                pitchValues.Add(ParseFloat(data[0])); // Position 0 of each array in the 2d array should have the pitch data, as mentioned in PitchData.cs
                pitchMenuTitles.Add(data[1]); // Position 1 of each array in the 2d array should have the name of the character, as mentioned in PitchData.cs
            }
        }

        public bool ToBool(int value)
        {
            return value != 0;
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
            var toggleVisibility = ToVisibility(toggle);

            button.IsEnabled = toggle;
            button.Visibility = toggleVisibility;
        }

        public MediaSource MediaSourceFromUri(string path)
        {
            return MediaSource.CreateFromUri(new Uri(path));
        }

        // Thanks StackOverflow man!
        public AppWindow GetAppWindowForCurrentWindow(object window)
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);

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

        public string GetAppVersion(bool forceBuildNumber = false)
        {
            var currentBuild = GetVersion();
            // We do a bit of array shenanigans (loving this word)
            return currentBuild[2] != 0 || forceBuildNumber ? $"{currentBuild[0]}.{currentBuild[1]}.{currentBuild[2]}" : $"{currentBuild[0]}.{currentBuild[1]}";
        }

        public string GetFormattedCurrentFile(string input)
        {
            return input.Replace(@"\", "/");
        }

        public async Task<bool> IsUpdateAvailable()
        {
            string url = "https://api.github.com/repos/lemons-studios/audio-replacer-2/releases/latest";
            string currentVersion = GetAppVersion(true);

            try
            {
                string json = await webRequest.GetWebRequest(url);
                webVersion = webRequest.SelectWebData(json); // GitHub API goes crazy

                return webVersion == currentVersion;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"An error occured while checking for updates: {ex}");
                return false;
            }
        }

        public string GetWebVersion()
        {
            return webVersion != string.Empty ? webVersion : GetAppVersion(); // App version used as fallback when no internet is available
        }

        private int[] GetVersion()
        {
            var appVersion = Package.Current.Id.Version;
            return [ appVersion.Major, appVersion.Minor, appVersion.Build ];
        }

        private float ParseFloat(string value)
        {
            try { return float.Parse(value); } catch { return 1; }
        }
    }
}
