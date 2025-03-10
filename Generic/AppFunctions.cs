using AudioReplacer.Util;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.Windows.AppLifecycle;
using Newtonsoft.Json.Linq;
using SevenZipExtractor;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace AudioReplacer.Generic;

public static class AppFunctions
{
    public static async Task DownloadDeps()
    {
        // Download FFmpeg
        var latestVersion = await GetWebData("https://www.gyan.dev/ffmpeg/builds/release-version");
        var ffmpegUrl = $"https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-{latestVersion}-full_build.7z";
        var outPath = Path.Join(AppProperties.ExtraApplicationData, "ffmpeg");
        await DownloadFileAsync(ffmpegUrl, $@"{AppProperties.ExtraApplicationData}\ffmpeg.7z");

        // Extract FFmpeg
        using (var ffmpegExtractor = new ArchiveFile($"{outPath}.7z")) ffmpegExtractor.Extract(outPath);

        // Move FFmpeg executable (ffmpeg.exe ONLY, ffprobe.exe and ffplay.exe are not needed) to the application's binary folder
        var info = new DirectoryInfo(outPath);
        foreach (var exe in info.GetFiles("ffmpeg.exe", SearchOption.AllDirectories))
        {
            File.Move(exe.FullName, Path.Combine(AppProperties.BinaryPath, exe.Name));
        }

        Directory.Delete(outPath, true);
        File.Delete($"{outPath}.7z");
    }

    [Log]
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

        try
        {
            if (autoStart)
            {
                // Attempt to start the process.
                if (!shellProcess.Start())
                {
                    throw new InvalidOperationException($"Failed to start process: {command} {args}");
                }

                // Wait for the process to exit.
                await shellProcess.WaitForExitAsync();

                // Check the exit code; if non-zero, throw an exception.
                if (shellProcess.ExitCode != 0)
                {
                    throw new Exception($"Process exited with code {shellProcess.ExitCode}. Command: {command} {args}");
                }
            }
        }
        catch (Exception ex)
        {
            // Optionally log the exception here if your [Log] attribute doesn't already do it.
            throw new Exception($"Error spawning process '{command}' with arguments '{args}': {ex.Message}", ex);
        }
    }

    [Log]
    public static async Task FfMpegCommand(string input, string command, string outPath)
    {
        await SpawnProcess(AppProperties.FfmpegPath, $"-i \"{input}\" {command} -y \"{outPath}\"");
    }

    public static void PopulateCustomData()
    {
        // Reset values of pitch/effect titles & values or initialize the properties
        AppProperties.PitchTitles = [];
        AppProperties.PitchValues = [];
        AppProperties.EffectTitles = [];
        AppProperties.EffectValues = [];

        // Add data to all the lists
        // AppProperties.PitchData gets automatically populated from the pitch/effect
        // json file, if the json serializer does not encounter any issues (i.e. invalid json)
        foreach (var data in AppProperties.PitchData)
        {
            AppProperties.PitchValues.Add(ParseFloat(data[0])); // Position 0 of each array in the 2d array should have the data
            AppProperties.PitchTitles.Add(data[1]); // Position 1 of each array in the 2d array should have the name of the property
        }
        foreach (var effects in AppProperties.EffectData)
        {
            // Same thing as pitch for effects
            AppProperties.EffectValues.Add(effects[0]);
            AppProperties.EffectTitles.Add(effects[1]);
        }
    }

    public static void OpenUrl(string url)
    {
        Task.Run(async () => await SpawnProcess("cmd", $"/c start {url}"));
    }

    // Config.NET does not allow boolean types with my setup, So these two boolean converter methods are needed.
    public static int BoolToInt(bool value)
    {
        // true = 1, false = 0.
        return value ? 1 : 0;
    }

    public static bool IntToBool(int value)
    {
        // If the value is not 1, return false. Makes sense ngl
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

    [Log]
    public static async Task<string> GetDataFromGithub(string url, string tagName)
    {
        try
        {
            var apiResponse = await AppProperties.WebClient.GetAsync(url);
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

    [Log]
    public static async Task<string> GetWebData(string url) // I sure do love stealing my own code!!
    {
        try
        {
            var response = await AppProperties.WebClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException)
        {
            return string.Empty;
        }
    }

    [Log]
    public static async Task DownloadFileAsync(string url, string outPath)
    {
        try
        {
            await using var webStream = await AppProperties.WebClient.GetStreamAsync(url);
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
