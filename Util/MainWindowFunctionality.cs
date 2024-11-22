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
using System.IO.Compression;
using System.IO;
using System.Net.Http;

namespace AudioReplacer2.Util
{
    // Easily removes a good chunk of code from MainWindow.xaml.cs. much easier to navigate through everything now!
    public class MainWindowFunctionality
    {
        public readonly WebRequest webRequest;

        private readonly InfoBar[] windowInfoBars;
        private readonly List<string> pitchMenuTitles;
        private readonly List<float> pitchValues;
        private readonly string webVersion;

        public MainWindowFunctionality(ComboBox pitchComboBox, InfoBar[] windowInfoBars)
        {
            webRequest = new WebRequest();
            pitchMenuTitles = [];
            pitchValues = [];
            this.windowInfoBars = windowInfoBars;

            foreach (var data in GlobalData.PitchData)
            {
                pitchValues.Add(ParseFloat(data[0])); // Position 0 of each array in the 2d array should have the pitch data, as mentioned in GlobalData.cs
                pitchMenuTitles.Add(data[1]); // Position 1 of each array in the 2d array should have the name of the character, as mentioned in GlobalData.cs
            }

            webVersion = Task.Run(webRequest.GetWebVersion).Result;
        }

        public void UpdateInfoBar(InfoBar infoBar, string title, string message, InfoBarSeverity severity, bool show = true, bool autoClose = true)
        {
            // Disable any active InfoBar before showing a new one
            DisableActiveInfoBars();
            infoBar.Title = title;
            infoBar.Message = message;


            infoBar.IsOpen = show;
            if (autoClose) Task.Run(() => WaitHideInfoBar(infoBar));
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
            bool returnBuildNumber = currentBuild[2] != 0 || forceBuildNumber;
            return returnBuildNumber ? $"{currentBuild[0]}.{currentBuild[1]}.{currentBuild[2]}" : $"{currentBuild[0]}.{currentBuild[1]}";
        }

        public string GetFormattedCurrentFile(string input)
        {
            return input.Replace(@"\", "/");
        }

        public bool IsUpdateAvailable()
        {
            try
            {
                return webVersion != GetAppVersion(true);
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

        private void DisableActiveInfoBars()
        {
            foreach (var infoBar in windowInfoBars)
            {
                if (infoBar.IsOpen) infoBar.IsOpen = false;
            }
        }

        private async Task WaitHideInfoBar(InfoBar infoBar)
        {
            await Task.Delay(1500);
            infoBar.DispatcherQueue.TryEnqueue(() =>
            {
                infoBar.IsOpen = false;
            });
        }

        public bool IsFFMpegInstalled()
        {
            // I know that using WinGet to install ffmpeg is not a great idea, especially for users who already have it installed
            // But let me tell you how much I hate trying to check for global installations on the path
            // It will be kept this way because frankly it just works
            // 100 extra megabytes idc anymore :(
            return File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Microsoft\\WinGet\\Links\\ffmpeg.exe");
        }


        public void DownloadDependencies()
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "cmd",
                    Arguments = "/c winget install ffmpeg --accept-source-agreements --accept-package-agreements",
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

        }

        public void UpdatePathEnvironmentVariable(string path)
        {
            Environment.SetEnvironmentVariable("PATH", $"{path};%PATH%", EnvironmentVariableTarget.User);
        }
    }
}
