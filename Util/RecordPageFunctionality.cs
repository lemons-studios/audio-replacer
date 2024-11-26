using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Core;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using System.Linq;
using SevenZipExtractor;

namespace AudioReplacer.Util
{
    public class RecordPageFunctionality
    {
        public readonly WebRequest webRequest;

        private readonly InfoBar[] windowInfoBars;
        private List<string> pitchMenuTitles = [];
        private List<float> pitchValues = [];
        private readonly string webVersion;

        public RecordPageFunctionality(InfoBar[] windowInfoBars)
        {
            webRequest = new WebRequest();
            this.windowInfoBars = windowInfoBars;
            webVersion = Task.Run(() => webRequest.GetWebVersion("https://api.github.com/repos/lemons-studios/audio-replacer-2/tags")).Result;
            UpdatePitchData();
        }

        public void UpdateInfoBar(InfoBar infoBar, string title, string message, InfoBarSeverity severity, bool show = true, bool autoClose = true)
        {
            DisableActiveInfoBars(); // Disable any active InfoBar before showing a new one
            infoBar.Title = title;
            infoBar.Message = message;
            infoBar.IsOpen = show;

            if (autoClose) Task.Run(() => WaitHideInfoBar(infoBar));
        }

        public void DownloadDependencies()
        {
            string latestFfMpegVersion = Task.Run(() => webRequest.GetWebData("https://www.gyan.dev/ffmpeg/builds/release-version")).Result;
            string ffMpegUrl = $"https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-{latestFfMpegVersion}-full_build.7z";
            string outPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Config";
            string fullOutPath = $"{outPath}\\ffmpeg";
            string currentSystemPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            webRequest.DownloadFile(ffMpegUrl, outPath, "ffmpeg.7z");
            using (ArchiveFile ffmpegArchive = new ArchiveFile($"{fullOutPath}.7z")) { ffmpegArchive.Extract($"{fullOutPath}"); }

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
                string pathEnv = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                if (string.IsNullOrEmpty(pathEnv)) return false;
                string[] paths = pathEnv.Split(Path.PathSeparator);
                return paths.Select(path => Path.Combine(path, "ffmpeg.exe")).Any(File.Exists);
            }
            catch { return false; }
        }

        public void ToggleButton(Button button, bool toggle)
        {
            var toggleVisibility = ToVisibility(toggle);

            button.IsEnabled = toggle;
            button.Visibility = toggleVisibility;
        }

        private void UpdatePitchData()
        {
            pitchMenuTitles = [];
            pitchValues = [];

            foreach (string[] data in GlobalData.DeserializedPitchData)
            {
                pitchValues.Add(ParseFloat(data[0])); // Position 0 of each array in the 2d array should have the pitch data, as mentioned in GlobalData.cs
                pitchMenuTitles.Add(data[1]); // Position 1 of each array in the 2d array should have the name of the character, as mentioned in GlobalData.cs
            }
        }

        public float GetPitchModifier(int index)
        {
            UpdatePitchData();
            try { return pitchValues[index]; } catch { return 1; }
        }

        public List<string> GetPitchTitles()
        {
            UpdatePitchData();
            return pitchMenuTitles;
        }

        private async Task WaitHideInfoBar(InfoBar infoBar)
        {
            await Task.Delay(GlobalData.NotificationTimeout);
            // This try-catch is needed in the case that the TryEnqueue is running while the window is closing
            try { infoBar.DispatcherQueue.TryEnqueue(() => { infoBar.IsOpen = false; }); }catch { /* ignored */ }
        }

        /// Yummy code minification..... I love making my code harder to read..... (on the other hand this reduced my line count by about 20-30 lines)
        private Visibility ToVisibility(bool x) { return x ? Visibility.Visible : Visibility.Collapsed; }
        public MediaSource MediaSourceFromUri(string path) { return MediaSource.CreateFromUri(new Uri(path)); }
        private void DisableActiveInfoBars() { foreach (var infoBar in windowInfoBars) { if (infoBar.IsOpen) infoBar.IsOpen = false; } }
        public string GetWebVersion() { return webVersion != string.Empty ? webVersion : GlobalData.GetAppVersion(); /* App version used as fallback when no internet is available*/ }
        public string GetFormattedCurrentFile(string input) { return input.Replace(@"\", "/"); }
        public bool IsUpdateAvailable() { try { return webVersion != GlobalData.GetAppVersion(true); } catch { return false; } }
        private float ParseFloat(string value) { try { return float.Parse(value); } catch { return 1; } }
        public bool ToBool(int value) { return value != 0; }
        public string BoolToYesNo(bool value) { return value ? "Yes" : "No"; }
    }
}
