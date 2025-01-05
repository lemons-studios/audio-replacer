using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage.Pickers;
using WinRT.Interop;
using AppInstance = Microsoft.Windows.AppLifecycle.AppInstance;

namespace AudioReplacer.Generic
{
    public static class AppGeneric // This generic class should pretty much have most backend stuff/data. UIGeneric should have most of the frontend stuff.
    {
        public static AppWindow AppWindow;
        public static bool UpdateChecksAllowed, InputRandomizationEnabled, ShowAudioEffectDetails, EnableFanfare;
        public static int NotificationTimeout, RecordStopDelay, RecordStartDelay;
        public static string[][] PitchData, EffectData;
        public static long richPresenceClientId = 1325340097234866297;


        public static string GetAppVersion(bool forceBuildNumber = false)
        {
            var appVersion = Package.Current.Id.Version;
            var version = $"{appVersion.Major}.{appVersion.Minor}";
            return forceBuildNumber || appVersion.Build != 0 ? $"{version}.{appVersion.Build}" : version;
        }

        // Microsoft genuinely amazes me. Folder pickers and File Open/Save pickers are all separate classes that inherit from object. No base "Picker" class or anything. Are they stupid or something?
        // My programming skills look like genuine genius when compared with the multi-trillion dollar company sometimes and that SAYS SOMETHING
        public static T CreateFolderPicker<T>(PickerLocationId startLoc, string fileTypeFiler) where T : class, new()
        {
            if (typeof(T) != typeof(FolderPicker) || typeof(T) != typeof(FileOpenPicker))
                throw new InvalidOperationException("This method only supports FolderPicker and FileOpenPicker");

            dynamic picker = new T();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(fileTypeFiler);
            return picker as T;
        }

        // From my research, the best way to do path selection is to use overload methods, even though it uses more lines when it really shouldn't
        public static async Task<string> PickDataPath(FolderPicker picker)
        {
            var pickerWindow = new Window();
            nint hwnd = WindowNative.GetWindowHandle(pickerWindow);

            InitializeWithWindow.Initialize(picker, hwnd);
            var result = await picker.PickSingleFolderAsync();
            return result.Path;
        }

        public static async Task<string> PickDataPath(FileOpenPicker picker)
        {
            var pickerWindow = new Window();
            nint hwnd = WindowNative.GetWindowHandle(pickerWindow);

            InitializeWithWindow.Initialize(picker, hwnd);
            var result = await picker.PickSingleFileAsync();
            return result.Path;
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
                true => x ? "yes" : "no",
                false => x.ToString()
            };
        }

        public static void CreateProcess(string command, string args, bool autoStart = true)
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
            shellProcess.Start();
        }

        public static void OpenURL(string url)
        {
            CreateProcess("cmd", $"/c start {url}");
        }

        public static void RestartApp()
        {
            AppInstance.Restart("Forced Restart");
        }
    }
}
