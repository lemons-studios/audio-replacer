﻿using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Whisper.net;

namespace AudioReplacer.Generic;
[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class AppProperties
{
    public static readonly string AppFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); // Test AppDomain if it works since this is way too long
    public static readonly string FfmpegPath = Path.Join(AppFolder, "ffmpeg.exe");

    public static readonly string ExtraApplicationData = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "audio-replacer");
    public static readonly string OutputPath = Path.Join(ExtraApplicationData, "out");

    public static readonly string WhisperPath = Path.Join(ExtraApplicationData, "whisper.bin");
    public static readonly string LogFile = Path.Join(ExtraApplicationData, "log.txt");

    public static readonly string ConfigPath = Path.Join(ExtraApplicationData, "config");
    public static readonly string SettingsFile = Path.Join(ConfigPath, "AppSettings.json");
    public static readonly string PitchDataFile = Path.Join(ConfigPath, "PitchData.json");
    public static readonly string EffectsDataFile = Path.Join(ConfigPath, "EffectsData.json");

    public static readonly bool IsWhisperInstalled = File.Exists(WhisperPath);

    public static bool InRecordState = false, IsAppLoaded = false;
    public static string[][] PitchData, EffectData;
    public static List<string> PitchTitles, EffectTitles, EffectValues;
    public static List<float> PitchValues;

    public static readonly HttpClient WebClient = new() { DefaultRequestHeaders = { { "User-Agent", "Audio Replacer" } } };

    // Only here because now it doesn't need to be defined each time. Only used in one spot, although I just want to keep it here since all other startup properties are here
    public static WhisperProcessor TranscriptionProcessor;
}
