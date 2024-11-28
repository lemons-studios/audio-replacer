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
        // Values here are the starting values and can be edited through the app's pitch config json file
        public static readonly string[][] DefaultPitchData =
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

        // Again, these values are only the starting values. They can be edited through their own effect config file, which also allows you to add/remove/modify these effects too; although you will need some knowledge of ffmpeg audio filters
        public static string[][] DefaultEffectData =
        [
            ["aecho=0.8:0.35:17", "Flashback"], 
            ["asplit=2[orig][low];[low]rubberband=pitch=0.8,volume=1.0,afftdn,acompressor[aud1];[orig][aud1]amix=inputs=", "Shadow Self"], 
            ["highpass=f=300, lowpass=f=3000, acrusher=level_in=1:bits=8:mode=log:level_out=0.8, acompressor=threshold=0.2:ratio=4:attack=50:release=30", "TV"],
            ["aecho=0.6:0.7:60:0.6, aecho=0.5:0.7:200:0.5, lowpass=f=500, bass=g=12, asetrate=44100*0.9, atempo=1.1, volume=1.2", "Godlike"],
            ["", "None"]
        ];

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
