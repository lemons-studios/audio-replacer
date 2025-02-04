using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace AudioReplacer.Generic;
public class AppProperties
{
    public static readonly string ExtraApplicationData = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "audio-replacer");
    public static readonly string BinaryPath = Path.Join(ExtraApplicationData, "bin");
    public static readonly string FfmpegPath = Path.Combine(BinaryPath, "ffmpeg.exe");
    public static readonly string WhisperPath = Path.Join(BinaryPath, "whisper.bin");
    public static readonly string ConfigPath = Path.Join(ExtraApplicationData, "config");
    public static readonly string SettingsFile = Path.Join(ConfigPath, "AppSettings.json");
    public static readonly string PitchDataFile = Path.Join(ConfigPath, "PitchData.json");
    public static readonly string EffectsDataFile = Path.Join(ConfigPath, "EffectsData.json");
    public static readonly string LogFile = Path.Join(ExtraApplicationData, "log.txt");
    public static readonly bool IsWhisperInstalled = File.Exists(WhisperPath);
    public static bool InRecordState = false, IsAppLoaded = false;
    public static string[][] PitchData, EffectData;
    public static List<string> PitchTitles, EffectTitles, EffectValues;
    public static List<float> PitchValues;

    public static readonly HttpClient WebClient = new()
    {
        DefaultRequestHeaders = { { "User-Agent", "Audio Replacer" } }
    };
}
