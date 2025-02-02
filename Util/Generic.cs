using Microsoft.UI.Composition.SystemBackdrops;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Windows.AppLifecycle;

namespace AudioReplacer.Util;
public class Generic
{
    public static readonly string ExtraApplicationData = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "audio-replacer");
    public static readonly string BinaryPath = Path.Join(ExtraApplicationData, "bin");
    public static readonly string FfmpegPath = Path.Combine(BinaryPath, "ffmpeg.exe");
    public static readonly string WhisperPath = Path.Join(BinaryPath, "whisper.bin");
    public static readonly string ConfigPath = Path.Join(ExtraApplicationData, "config");
    public static readonly string SettingsFile = Path.Join(ConfigPath, "AppSettings.json");
    public static readonly string PitchDataFile = Path.Join(ConfigPath, "PitchData.json");
    public static readonly string EffectsDataFile = Path.Join(ConfigPath, "EffectsData.json");
    public static readonly string LogFile = Path.Join(ExtraApplicationData, "logs", $"audioReplacer-log-{DateTime.Now.ToString("dd-MM-yyyy HH:mm")}");
    public static readonly bool IsWhisperInstalled = File.Exists(WhisperPath);
    public static bool InRecordState = false, IsAppLoaded = false;
    public static string[][] PitchData, EffectData;
    public static List<string> PitchTitles, EffectTitles, EffectValues;
    public static List<float> PitchValues;

    private static readonly HttpClient WebClient = new()
    {
        DefaultRequestHeaders = {{ "User-Agent", "Audio Replacer" }}
    };

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

    public static void PopulateCustomData()
    {
        PitchTitles = [];
        PitchValues = [];
        EffectTitles = [];
        EffectValues = [];
        foreach (var data in PitchData)
        {
            PitchValues.Add(ParseFloat(data[0])); // Position 0 of each array in the 2d array should have the data
            PitchTitles.Add(data[1]); // Position 1 of each array in the 2d array should have the name of the property
        }
        foreach (var effects in EffectData)
        {
            EffectValues.Add(effects[0]);
            EffectTitles.Add(effects[1]);
        }
    }

    public static void OpenUrl(string url)
    {
        Task.Run(async() => await SpawnProcess("cmd", $"/c start {url}"));
    }

    // Config.NET does not allow boolean types with my setup, So these two boolean converter methods are needed.
    public static int BoolToInt(bool value)
    {
        // true = 1, false = 0.
        return value ? 1 : 0;
    }

    public static bool IntToBool(int value)
    {
        // If the value is not 1, return a false boolean.
        return value == 1;
    }

    public static void RestartApp()
    {
       AppInstance.Restart("");
    }

    public static int GetTransparencyMode()
    {
        return MicaController.IsSupported() ? 0 : 1;
    }

    public static float ParseFloat(string value)
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

    public static async Task<string> GetDataFromGithub(string tagName)
    {
        var url = "https://api.github.com/repos/lemons-studios/audio-replacer/releases/latest";
        try
        {
            var apiResponse = await WebClient.GetAsync(url);
            if (!apiResponse.IsSuccessStatusCode) return "Error Getting Tag Information";

            var responseData = await apiResponse.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseData);
            var tagInfo = json[tagName]?.ToString();
            return string.IsNullOrEmpty(tagInfo) ? "Error Getting Tag Information" : tagInfo;
        }
        catch
        {
            return "Error Getting Tag Information";
        }
    }

    public static async Task<string> GetWebData(string url) // I sure do love stealing my own code!!
    {
        try
        {
            var response = await WebClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException) { return string.Empty; }
    }

    public static async Task DownloadFileAsync(string url, string outPath)
    {
        try
        {
            await using var webStream = await WebClient.GetStreamAsync(url);
            await using var fileStream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            await webStream.CopyToAsync(fileStream);
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while downloading the file.", e);
        }
    }

    public static string GetAppVersion(bool forceBuildNumber = false)
    {
        var splitVer = Assembly.GetEntryAssembly()!.GetName()!.Version!.ToString().Split(".");
        var major = splitVer[0];
        var minor = splitVer[1];
        var build = splitVer[2];

        if (forceBuildNumber || build != "0")
        {
            return $"{major}.{minor}.{build}";
        }
        else
        {
            return $"{major}.{minor}";
        }
    }
} 
