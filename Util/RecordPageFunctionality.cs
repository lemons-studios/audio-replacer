using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Core;
using Microsoft.UI.Xaml.Controls;
using System.IO;

namespace AudioReplacer2.Util
{
    public class RecordPageFunctionality
    {
        public readonly WebRequest webRequest;

        private readonly InfoBar[] windowInfoBars;
        private readonly List<string> pitchMenuTitles;
        private readonly List<float> pitchValues;
        private readonly string webVersion;

        public RecordPageFunctionality(ComboBox pitchComboBox, InfoBar[] windowInfoBars)
        {
            webRequest = new WebRequest();
            pitchMenuTitles = [];
            pitchValues = [];
            this.windowInfoBars = windowInfoBars;

            foreach (var data in GlobalData.pitchData)
            {
                pitchValues.Add(ParseFloat(data[0])); // Position 0 of each array in the 2d array should have the pitch data, as mentioned in GlobalData.cs
                pitchMenuTitles.Add(data[1]); // Position 1 of each array in the 2d array should have the name of the character, as mentioned in GlobalData.cs
            }
            webVersion = Task.Run(() => webRequest.GetWebVersion("https://api.github.com/repos/lemons-studios/audio-replacer-2/tags")).Result;
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

        public void DownloadDependencies()
        {
            var downloadProcess = ShellCommandManager.CreateProcess("winget", "install ffmpeg --accept-source-agreements --accept-package-agreements", true, false, false, true);
            downloadProcess.Start();
            downloadProcess.WaitForExit();
        }

        public bool IsFfMpegAvailable()
        {
            // Get path variable from system and loop through all of it to check if the path contains ffmpeg.exe
            // This also allows for installs that don't come from winget (such as ffmpeg installed from Chocolatey or a manually installed copy of ffmpeg)
            try
            {
                var pathEnv = Environment.GetEnvironmentVariable("PATH");
                if (string.IsNullOrEmpty(pathEnv)) return false;
                
                var paths = pathEnv.Split(Path.PathSeparator);

                foreach (var path in paths)
                {
                    var ffmpegPath = Path.Combine(path, "ffmpeg.exe");
                    if (File.Exists(ffmpegPath)) return true;
                }
                return false;
            }
            catch { return false; }
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

        public List<string> GetPitchTitles()
        {
            return pitchMenuTitles;
        }

        public float GetPitchModifier(int index)
        {
            try { return pitchValues[index]; } catch { return 1; }
        }

        public string GetFormattedCurrentFile(string input)
        {
            return input.Replace(@"\", "/");
        }

        public bool IsUpdateAvailable()
        {
            try
            {
                return webVersion != GlobalData.GetAppVersion(true);
            }
            catch
            {
                return false;
            }
        }

        public string GetWebVersion()
        {
            return webVersion != string.Empty ? webVersion : GlobalData.GetAppVersion(); // App version used as fallback when no internet is available
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
            await Task.Delay(GlobalData.notificationTimeout);

            // This try-catch is needed in the case that the TryEnqueue is running while the window is closing
            try
            {
                infoBar.DispatcherQueue.TryEnqueue(() =>
                {
                    infoBar.IsOpen = false;
                });
            }
            catch { return; }
        }
    }
}
