using Config.Net;

namespace AudioReplacer.Util;

public interface IAppSettings
{
    [Option(Alias = "Theme", DefaultValue = 0)]
    int AppThemeSetting { get; set; }

    [Option(Alias = "TransparencyEffect", DefaultValue = 0)]
    int AppTransparencySetting { get; set; }

    [Option(Alias = "EnableUpdateChecks", DefaultValue = 1)]
    int AppUpdateCheck { get; set; }

    [Option(Alias = "RecordEndWaitTime", DefaultValue = 75)]
    int RecordEndWaitTime { get; set; }

    [Option(Alias = "NotificationTimeout", DefaultValue = 1750)]
    int NotificationTimeout { get; set; }

    [Option(Alias = "RememberSelectedFolder", DefaultValue = 1)]
    int RememberSelectedFolder { get; set; }

    [Option(Alias = "LastSelectedFolder", DefaultValue = "")]
    string LastSelectedFolder { get; set; }

    [Option(Alias = "InputRandomizationEnabled", DefaultValue = 0)]
    int InputRandomizationEnabled { get; set; }

    [Option(Alias = "RecordStartWaitTime", DefaultValue = 25)]
    int RecordStartWaitTime { get; set; }

    [Option(Alias = "EnableRichPresence", DefaultValue = 1)]
    int EnableRichPresence { get; set; }

    [Option(Alias = "EnableTranscription", DefaultValue = 1)]
    int EnableTranscription { get; set; }

    [Option(Alias = "AutoConvertFiles", DefaultValue = 1)]
    int AutoConvertFiles { get; set; }
}
