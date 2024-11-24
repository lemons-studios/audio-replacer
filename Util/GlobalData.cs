using Windows.ApplicationModel;
using Microsoft.UI.Windowing;

namespace AudioReplacer2.Util
{
    public static class GlobalData
    {
        public static AppWindow appWindow;
        public static bool updateChecksAllowed = true;

        public static string GetAppVersion(bool forceBuildNumber = false)
        {
            var appVersion = Package.Current.Id.Version;
            int[] currentBuild = [appVersion.Major, appVersion.Minor, appVersion.Build];

            // We do a bit of array shenanigans (loving this word)
            bool returnBuildNumber = currentBuild[2] != 0 || forceBuildNumber;
            return returnBuildNumber ? $"{currentBuild[0]}.{currentBuild[1]}.{currentBuild[2]}" : $"{currentBuild[0]}.{currentBuild[1]}";
        }

        // I love my 2d arrays
        public static string[][] defaultSettingValues = [["2", "appTheme"], ["0", "appTransparency"], ["1", "appUpdateChecks"], ["1550", "toastTimeout"], ["75", "recordStopDelay"]];

        // Modify THIS variable to add, remove, or modify default pitch values.
        // Use multiplicative values for pitch values (for example, if you want to reduce the pitch of the recording by 4%, you would want to enter in "0.96"
        // TODO: switch to a real JSON file to configure json data and use this pitchData string as the default values. Users can configure pitch data using that file
        public static string[][] pitchData =
        [
            ["1.0075", "Ai Ebihara"], ["1.025", "Ayane Matsunaga"], ["0.555", "Ameno-Sagiri"], ["1.015", "Chie Satonaka"],
            ["1.012", "Chihiro Fushimi"], ["0.9925", "Daisuke Nagase"], ["1.0195", "Eri Minami"], ["1.0065", "Hanako Ohtani"],
            ["1.0", "Igor"],
            ["1.0085", "Izanami"], ["0.9970", "Kanji Tatsumi"], ["0.9875", "Kinshiro Morooka"], ["0.9975", "Kou Ichijo"],
            ["0.9825", "Kunino-Sagiri"],
            ["1.025", "Kusumi-no-Okami"], ["1.0175", "Margaret"], ["1.02", "Marie"], ["0.981", "Mitsuo Kubo"],
            ["1.03", "Nanako Dojima"],
            ["0.984", "Naoki Konishi"], ["1.0125", "Naoto Shirogane"], ["1.01575", "Noriko Kashiwagi"],
            ["0.98", "Principal (Gekkoukan)"],
            ["0.98", "Principal (Yasogami)"], ["1.01675", "Rise Kujikawa"], ["0.9845", "Ryotaro Dojima"],
            ["1.0225", "Saki Konishi"],
            ["1.015", "Sayoko Uehara"], ["1.00175", "Shu Nakajima"], ["0.9835", "Taro Namatame"], ["1", "Teddie"],
            ["0.975", "Tohru Adachi"],
            ["1.0165", "Yukiko Amagi"], ["0.9975", "Yosuke Hanamura"], ["0.9875", "Yu Narukami"], ["1.0135", "Yumi Ozawa"],
            ["1.00", "Other NPC"]
        ];
    }
}
