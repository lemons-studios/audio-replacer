using Microsoft.UI.Composition.SystemBackdrops;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AudioReplacer.Util;

public class Generic
{
    public static string extraApplicationData = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "audio-replacer");
    public static string binaryPath = Path.Join(extraApplicationData, "bin");
    public static string ffmpegPath = Path.Combine(binaryPath, "ffmpeg.exe");
    public static string whisperPath = Path.Join(binaryPath, "whisper.bin");
    public static string configPath = Path.Join(extraApplicationData, "config");
    public static bool isAppLoaded = false;
    public static string SettingsFile = Path.Join(configPath, "AppSettings.json");
    public static string PitchDataFile = Path.Join(configPath, "PitchData.json");
    public static string EffectsDataFile = Path.Join(configPath, "EffectsData.json");
    public static string[][] PitchData;
    public static string[][] EffectData;
    public static bool InRecordState;

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
        EffectValues = [];
        EffectTitles = [];
        
        foreach (string[] data in PitchData)
        {
            PitchValues.Add(ParseFloat(data[0])); // Position 0 of each array in the 2d array should have the data
            PitchTitles.Add(data[1]); // Position 1 of each array in the 2d array should have the name of the property
        }
        foreach (string[] effects in EffectData)
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

    public static async Task<string> GetWebVersion(string url)
    {
        try
        {
            var apiResponse = await WebClient.GetAsync(url);
            if (!apiResponse.IsSuccessStatusCode) throw new Exception($"API responded with status code {apiResponse.StatusCode}");
            string responseData = await apiResponse.Content.ReadAsStringAsync();

            var jsonTags = JArray.Parse(responseData);
            if (jsonTags.Count == 0) throw new Exception("No valid tags found in response data");

            string name = jsonTags[0]["name"]?.ToString();
            if (string.IsNullOrEmpty(name)) throw new Exception("The 'name' property is missing or empty in the first tag.");
            return name;
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse JSON: {ex}");
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred during the web request: {ex.Message}");
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
