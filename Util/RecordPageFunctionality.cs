using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Core;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SevenZipExtractor;
using AudioReplacer.Generic;

namespace AudioReplacer.Util
{
    public class RecordPageFunctionality
    {
        private readonly WebRequest webRequest;

        private readonly InfoBar[] windowInfoBars;
        private List<string> pitchMenuTitles, effectMenuTitles, effectMenuValues;
        private List<float> pitchValues;
        private readonly string webVersion;

        public RecordPageFunctionality(InfoBar[] windowInfoBars)
        {
            webRequest = new WebRequest();
            this.windowInfoBars = windowInfoBars;
            try
            {
                webVersion = Task.Run(() => webRequest.GetWebVersion("https://api.github.com/repos/lemons-studios/audio-replacer-2/tags")).Result;
            }
            catch (AggregateException) // Typically occurs when GitHub is queried too much within a specific period of time from the same ip address. Should not affect the application aside from update checks
            {
                webVersion = AppGeneric.GetAppVersion(true); 
            }
            UpdatePitchData();
        }

        public void UpdateInfoBar(InfoBar infoBar, string title, string message, InfoBarSeverity severity, bool show = true, bool autoClose = true)
        {
            DisableActiveInfoBars(); // Disable any active InfoBar before showing a new one
            try
            {
                infoBar.Title = title;
                infoBar.Message = message;
                infoBar.IsOpen = show;
                if (autoClose) Task.Run(() => WaitHideInfoBar(infoBar));
            }
            catch (COMException)
            { /*This just happens for some reason, no errors arise from it though. This catch exception is here to prevent the app from crashing for no reason*/ }
        }

        public void DownloadDependencies()
        {
            string latestFfMpegVersion = Task.Run(() => webRequest.GetWebData("https://www.gyan.dev/ffmpeg/builds/release-version")).Result;
            string ffMpegUrl = $"https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-{latestFfMpegVersion}-full_build.7z";
            string outPath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\audio-replacer\";
            string fullOutPath = $@"{outPath}\ffmpeg";
            string currentSystemPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            webRequest.DownloadFile(ffMpegUrl, outPath, "ffmpeg.7z");
            using (ArchiveFile ffmpegArchive = new ArchiveFile($"{fullOutPath}.7z")) { ffmpegArchive.Extract($"{fullOutPath}"); }

            Directory.Move(@$"{fullOutPath}\ffmpeg-{latestFfMpegVersion}-full_build\bin", @$"{outPath}\ffmpeg-bin");
            string updatedPath = $"{currentSystemPath};{Path.Combine(outPath, "ffmpeg-bin")}";
            Environment.SetEnvironmentVariable("PATH", updatedPath, EnvironmentVariableTarget.User);

            // Delete both the downloaded 7z archive and the ffmpeg folder it came in
            File.Delete($"{fullOutPath}.7z");
            Directory.Delete($"{fullOutPath}", true);
        }

        public bool IsFfMpegAvailable()
        {
            // Get path variable from system and loop through all of it to check if the path contains ffmpeg.exe
            try
            {
                string pathEnv = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                if (string.IsNullOrEmpty(pathEnv)) return false;
                string[] paths = pathEnv.Split(Path.PathSeparator);
                return paths.Select(path => Path.Combine(path, "ffmpeg.exe")).Any(File.Exists);
            }
            catch
            {
                return false;
            }
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
            effectMenuValues = [];
            effectMenuTitles = [];

            foreach (string[] data in AppGeneric.PitchData)
            {
                pitchValues.Add(ParseFloat(data[0])); // Position 0 of each array in the 2d array should have the pitch data, as mentioned in GlobalData.cs
                pitchMenuTitles.Add(data[1]); // Position 1 of each array in the 2d array should have the name of the character, as mentioned in GlobalData.cs
            }
            foreach (string[] effects in AppGeneric.EffectData)
            {
                effectMenuValues.Add(effects[0]);
                effectMenuTitles.Add(effects[1]);
            }
        }

        private async Task WaitHideInfoBar(InfoBar infoBar)
        {
            await Task.Delay(AppGeneric.NotificationTimeout);
            // This try-catch is needed in the case that the TryEnqueue is running while the window is closing
            try { infoBar.DispatcherQueue.TryEnqueue(() => { infoBar.IsOpen = false; }); }catch { /* ignored */ }
        }

        private Visibility ToVisibility(bool x)
        {
            return x ? Visibility.Visible : Visibility.Collapsed;
        }

        public MediaSource MediaSourceFromUri(string path)
        {
            return MediaSource.CreateFromUri(new Uri(path));
        }

        private void DisableActiveInfoBars()
        {
            try
            {
                foreach (var infoBar in windowInfoBars)
                {
                    if (infoBar.IsOpen) infoBar.IsOpen = false;
                }
            }
            catch (COMException)
            { /*Another instance of an InfoBar crashing the app for no reason whatsoever*/ }
        }

        public string BoolToString(bool? value, bool humanizeOutput = true)
        {
            return humanizeOutput switch
            {
                true => value != null && (bool) value ? "Yes" : "No",
                false => value.ToString()
            };
        }

        public string GetWebVersion()
        {
            return webVersion != string.Empty ? webVersion : AppGeneric.GetAppVersion(); /* App version used as fallback when no internet is available*/
        }

        public string GetFormattedCurrentFile(string input)
        {
            return input.Replace(@"\", "/");
        }

        public bool IsUpdateAvailable()
        {
            try
            {
                return webVersion != AppGeneric.GetAppVersion(true);
            }
            catch
            {
                return false;
            }
        }

        private float ParseFloat(string value)
        {
            try
            {
                return float.Parse(value);
            }
            catch
            {
                return 1;
            }
        }

        public bool FolderMemoryAllowed()
        {
            return App.AppSettings.RememberSelectedFolder == 1 && Path.Exists(App.AppSettings.LastSelectedFolder);
        }

        public float GetPitchModifier(int index)
        {
            try
            {
                return pitchValues[index];
            }
            catch
            {
                return 1;
            }
        }

        public string GetEffectValues(int index)
        {
            return effectMenuValues[index];
        }

        public List<string> GetPitchTitles()
        {
            return pitchMenuTitles;
        }

        public List<string> GetEffectTitles()
        {
            return effectMenuTitles;
        }
    }
}
