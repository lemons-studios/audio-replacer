using Microsoft.UI.Windowing;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage.Pickers;
using AppInstance = Microsoft.Windows.AppLifecycle.AppInstance;

namespace AudioReplacer.Generic
{
    public static class AppGeneric // This generic class should pretty much have most backend stuff/data. UIGeneric should have most of the frontend stuff.
    {
        public static AppWindow AppWindow;
        public static bool UpdateChecksAllowed, InputRandomizationEnabled, ShowAudioEffectDetails, EnableFanfare;
        public static int NotificationTimeout, RecordStopDelay, RecordStartDelay;
        public static string[][] PitchData, EffectData;

        public static string GetAppVersion(bool forceBuildNumber = false)
        {
            var appVersion = Package.Current.Id.Version;
            var version = $"{appVersion.Major}.{appVersion.Minor}";
            return forceBuildNumber || appVersion.Build != 0 ? $"{version}.{appVersion.Build}" : version;
        }

        public static bool IntToBool(int x)
        {
            // Any value aside from 1 is treated as false. This is intentional
            return x == 1;
        }

        public static int BoolToInt(bool x)
        {
            // If bool x is true, return 1. If bool x is false, return 0.
            return x ? 1 : 0;
        }

        public static string BoolToString(bool x, bool naturalLanguageOutput = false)
        {
            return naturalLanguageOutput switch
            {
                true => x ? "Yes" : "No",
                false => x.ToString()
            };
        }

        public static async Task SpawnProcess(string command, string args, bool autoStart = true)
        {
            var shellProcess = new Process
            {
                StartInfo =
                {
                    FileName = command,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    RedirectStandardInput = false,
                    CreateNoWindow = true
                }
            };
            if (autoStart)
            {
                shellProcess.Start();
                await shellProcess.WaitForExitAsync();
            }
        }

        public static void OpenUrl(string url)
        {
           Task.Run(() => SpawnProcess("cmd", $"/c start {url}"));
        }

        public static void RestartApp()
        {
            AppInstance.Restart("");
        }
    }
}
