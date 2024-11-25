using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Core;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using SevenZipExtractor;

namespace AudioReplacer2.Util
{
    public class RecordPageFunctionality
    {
        public readonly WebRequest webRequest;

        private readonly InfoBar[] windowInfoBars;
        private readonly List<string> pitchMenuTitles = [];
        private readonly List<float> pitchValues = [];
        private readonly string webVersion;

        public RecordPageFunctionality(ComboBox pitchComboBox, InfoBar[] windowInfoBars)
        {
            webRequest = new WebRequest();
            this.windowInfoBars = windowInfoBars;
            UpdatePitchData();

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
            var latestFfMpegVersion = Task.Run(() => webRequest.GetWebData("https://www.gyan.dev/ffmpeg/builds/release-version")).Result;
            var ffMpegUrl = $"https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-{latestFfMpegVersion}-full_build.7z";
            var outPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Config";
            var fullOutPath = $"{outPath}\\ffmpeg";
            var currentSystemPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            webRequest.DownloadFile(ffMpegUrl, outPath, "ffmpeg.7z");

            using (ArchiveFile ffmpegArchive = new ArchiveFile($"{fullOutPath}.7z"))
            {
                ffmpegArchive.Extract($"{fullOutPath}");
            }

            Directory.Move(@$"{fullOutPath}\ffmpeg-{latestFfMpegVersion}-full_build\bin", @$"{outPath}\ffmpeg-bin");
            Environment.SetEnvironmentVariable("PATH", @$"{currentSystemPath};{outPath}\ffmpeg-bin\", EnvironmentVariableTarget.User);

            // Delete both the downloaded 7z archive and the ffmpeg folder it came in
            File.Delete($"{fullOutPath}.7z");
            Directory.Delete($"{fullOutPath}", true); // Setting the second parameter to true also deletes all files in the folder, which is needed to occur
        }

        public bool IsFfMpegAvailable()
        {
            // Get path variable from system and loop through all of it to check if the path contains ffmpeg.exe
            // This also allows for installs that don't come from winget (such as ffmpeg installed from Chocolatey or a manually installed copy of ffmpeg)
            try
            {
                var pathEnv = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
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
            UpdatePitchData();
            return pitchMenuTitles;
        }

        public float GetPitchModifier(int index)
        {
            UpdatePitchData();
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

        public void UpdatePitchData()
        {
            foreach (var data in GlobalData.deserializedPitchData)
            {
                pitchValues.Add(ParseFloat(data[0])); // Position 0 of each array in the 2d array should have the pitch data, as mentioned in GlobalData.cs
                pitchMenuTitles.Add(data[1]); // Position 1 of each array in the 2d array should have the name of the character, as mentioned in GlobalData.cs
            }
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
    }
}
