using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Whisper.net;

namespace AudioReplacer.Generic;
[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class AppProperties
{
    public static readonly string ExtraApplicationData = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "audio-replacer");
    public static readonly string BinaryPath = Path.Join(ExtraApplicationData, "bin");
    public static readonly string FfmpegPath = Path.Combine(BinaryPath, "ffmpeg.exe");
    public static readonly string WhisperPath = Path.Join(BinaryPath, "whisper.bin");
    public static readonly string ConfigPath = Path.Join(ExtraApplicationData, "config");
    public static readonly string OutputPath = Path.Join(ExtraApplicationData, "out");
    public static readonly string SettingsFile = Path.Join(ConfigPath, "AppSettings.json");
    public static readonly string PitchDataFile = Path.Join(ConfigPath, "PitchData.json");
    public static readonly string EffectsDataFile = Path.Join(ConfigPath, "EffectsData.json");
    public static readonly string LogFile = Path.Join(ExtraApplicationData, "log.txt");
    public static readonly bool IsWhisperInstalled = File.Exists(WhisperPath);

    public static bool InRecordState = false, IsAppLoaded = false;
    public static string[][] PitchData, EffectData;
    public static List<string> PitchTitles, EffectTitles, EffectValues;
    public static List<float> PitchValues;

    public static readonly HttpClient WebClient = new() { DefaultRequestHeaders = { { "User-Agent", "Audio Replacer" } } };

    public static readonly WhisperFactory WhisperFactory = WhisperFactory.FromPath(WhisperPath);
    public static WhisperProcessor TranscriptionProcessor = WhisperFactory.CreateBuilder()
        .WithLanguage("auto")
        .WithTranslate() // Why the hell not
        .Build();
}
