using Windows.ApplicationModel;
using Microsoft.UI.Windowing;

namespace AudioReplacer.Util
{
    public static class GlobalData
    {
        public static AppWindow AppWindow;
        public static bool UpdateChecksAllowed = true;
        public static int NotificationTimeout, RecordStopDelay;
        public static string[][] DeserializedPitchData, DeserializedEffectData;

        public static string GetAppVersion(bool forceBuildNumber = false)
        {
            var appVersion = Package.Current.Id.Version;
            int[] currentBuild = [appVersion.Major, appVersion.Minor, appVersion.Build];

            // We do a bit of array shenanigans (loving this word)
            bool returnBuildNumber = currentBuild[2] != 0 || forceBuildNumber;
            return returnBuildNumber ? $"{currentBuild[0]}.{currentBuild[1]}.{currentBuild[2]}" : $"{currentBuild[0]}.{currentBuild[1]}";
        }
    }
}
