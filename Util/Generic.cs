using Microsoft.UI.Composition.SystemBackdrops;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AudioReplacer.Util;

public class Generic
{
    public static string extraApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "audio-replacer");
    public static string configPath = Path.Combine(extraApplicationData, "config");
    public static bool isAppLoaded = false;
    public static string SettingsFile = Path.Combine(configPath, "AppSettings.json");
    public static string PitchDataFile = Path.Combine(configPath, "PitchData.json");
    public static string EffectsDataFile = Path.Combine(configPath, "EffectsData.json");
    public static string[][] PitchData;
    public static string[][] EffectData;
    public static bool InRecordState;

    public static List<string> pitchMenuTitles, effectMenuTitles, effectMenuValues;
    public static List<float> pitchValues;

    private static readonly HttpClient WebClient = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "Audio Replacer" }
        }
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
        pitchMenuTitles = [];
        pitchValues = [];
        effectMenuValues = [];
        effectMenuTitles = [];


        foreach (string[] data in PitchData)
        {
            pitchValues.Add(ParseFloat(data[0])); // Position 0 of each array in the 2d array should have the pitch data, as mentioned in GlobalData.cs
            pitchMenuTitles.Add(data[1]); // Position 1 of each array in the 2d array should have the name of the character, as mentioned in GlobalData.cs
        }
        foreach (string[] effects in EffectData)
        {
            effectMenuValues.Add(effects[0]);
            effectMenuTitles.Add(effects[1]);
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

    public static void DownloadFile(string url, string outPath, string outName)
    {
        try
        {
            using var webStream = WebClient.GetStreamAsync(url);
            using var fileStream = new FileStream($"{outPath}\\{outName}", FileMode.OpenOrCreate);
            webStream.Result.CopyTo(fileStream);
        }
        catch (AggregateException e)
        {
            throw new AggregateException(e);
        }
    }
}

