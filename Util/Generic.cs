using Microsoft.UI.Composition.SystemBackdrops;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace AudioReplacer.Util;

public class Generic
{
    public static readonly string ExtraApplicationData = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "audio-replacer");
    public static readonly string BinaryPath = Path.Join(ExtraApplicationData, "bin");
    public static readonly string FfmpegPath = Path.Combine(BinaryPath, "ffmpeg.exe");
    public static readonly string whisperPath = Path.Join(BinaryPath, "whisper.bin");
    public static readonly string ConfigPath = Path.Join(ExtraApplicationData, "config");
    public static bool IsAppLoaded = false;
    public static readonly string SettingsFile = Path.Join(ConfigPath, "AppSettings.json");
    public static readonly string PitchDataFile = Path.Join(ConfigPath, "PitchData.json");
    public static readonly string EffectsDataFile = Path.Join(ConfigPath, "EffectsData.json");
    public static string[][] PitchData;
    public static string[][] EffectData;
    public static bool InRecordState;

    public static List<string> PitchTitles, EffectTitles, EffectValues;
    public static List<float> PitchValues;

    public static readonly bool IsWhisperInstalled = File.Exists(whisperPath);

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
        EffectValues = [];
        EffectTitles = [];
        
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

    public static int GenerateRandomNumber(int min, int max)
    {
        var rng = new Random((int) DateTime.Now.Ticks);
        return rng.Next(min, max);
    }

    public static void OpenUrl(string url)
    {
        Task.Run(async() => await SpawnProcess("cmd", $"/c start {url}"));
    }

    public static int BoolToInt(bool value)
    {
        return value ? 1 : 0;
    }

    public static bool IntToBool(int value)
    {
        // If the value is not 1, return a false boolean
        return value == 1;
    }

    public static void RestartApp()
    {
        Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
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
        string url = "https://api.github.com/repos/lemons-studios/audio-replacer/releases/latest";
        try
        {
            var apiResponse = await WebClient.GetAsync(url);
            if (!apiResponse.IsSuccessStatusCode)
                return "# Error getting release notes.";
            string responseData = await apiResponse.Content.ReadAsStringAsync();

            var json = JObject.Parse(responseData);
            string tagInfo = json[tagName]?.ToString();
            if (string.IsNullOrEmpty(tagInfo))
                return "# Error getting release notes.";

            return tagInfo;
        }
        catch
        {
            return "# Error getting release notes.";
        }
    }

    public static async Task<string> GetWebData(string url) // I sure do love stealing my own code!!
    {
        using HttpClient httpClient = new HttpClient();
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException) { return string.Empty; }
    }

    public static async Task DownloadFileAsync(string url, string outPath)
    {
        try
        {
            using var webStream = await WebClient.GetStreamAsync(url);
            using var fileStream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            await webStream.CopyToAsync(fileStream);
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while downloading the file.", e);
        }
    }

    public static string GetAppVersion(bool forceBuildNumber = false)
    {
        try
        {
            var fullVer = Assembly.GetEntryAssembly().GetName().Version.ToString();
            string[] splitVer = fullVer.Split(".");
            return !forceBuildNumber ? $"{splitVer[0]}.{splitVer[1]}" : $"{splitVer[0]}.{splitVer[1]}.{splitVer[2]}";
        }
        catch (NullReferenceException)
        {
            return string.Empty;
        }
    }
} 
