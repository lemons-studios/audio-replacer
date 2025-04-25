using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.Windows.AppLifecycle;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Whisper.net.LibraryLoader;
using System.Threading;
using Whisper.net;

// ReSharper disable MemberCanBePrivate.Global
namespace AudioReplacer.Generic;

public static class AppFunctions
{
    public static async Task DownloadFfmpeg()
    {
        // Download FFmpeg (using my custom ffmpeg build)
        const string url = $"https://github.com/lemons-studios/audio-replacer-ffmpeg/releases/latest/download/ffmpeg.exe";
        var executable = Path.Join(AppProperties.ExtraApplicationData, "ffmpeg.exe");
        await DownloadFileAsync(url, executable);
        File.Move(executable, AppProperties.FfmpegPath, overwrite: true);
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
    public static async Task<string> TranscribeFile(string path)
    {
        if (AppProperties.TranscriptionProcessor == null)
        {
            // Initializing this at startup would crash the app if whisper wasn't present. This will initialize it on the first transcription on each app launch
            AppProperties.TranscriptionProcessor = WhisperFactory.FromPath(AppProperties.WhisperPath).CreateBuilder()
                .WithLanguage("auto")
                .WithTranslate() // Why the hell not
                .Build();
        }

        var output = string.Empty;
        // Determine the best runtime for transcription
        RuntimeOptions.RuntimeLibraryOrder = [RuntimeLibrary.Cuda, RuntimeLibrary.Vulkan, RuntimeLibrary.Cpu, RuntimeLibrary.CpuNoAvx];
        var loadedLib = RuntimeOptions.LoadedLibrary;

        var timeout = loadedLib switch
        {
            RuntimeLibrary.Cuda => 3000,
            RuntimeLibrary.Vulkan => 12500,
            RuntimeLibrary.Cpu => 20000,
            _ => 35000
        };

        var cts = new CancellationTokenSource(timeout);
        try
        {
            // Do some funky wizardry to make the file work with Whisper.NET
            await using var fileStream = File.OpenRead(path);
            using var wavStream = new MemoryStream();
            await using var reader = new WaveFileReader(fileStream);
            var resamplingProcessor = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
            WaveFileWriter.WriteWavFileToStream(wavStream, resamplingProcessor.ToWaveProvider16());
            wavStream.Seek(0, SeekOrigin.Begin);

            // Process audio file
            await foreach (var result in AppProperties.TranscriptionProcessor.ProcessAsync(wavStream, cts.Token))
            {
                output = result.Text;
            }
            return $"Transcription: {output}";
        }
        catch (OperationCanceledException)
        {
            // This sometimes happens on shorter files. Time out after 30s
            return "Transcription Timed out";
        }
        catch (Exception e)
        {
            return $"Could Not Transcribe Contents: {e.Message}. Please open an issue on the project repository";
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

    // Config.NET does not allow boolean types whatsoever, So The following two boolean converter methods are needed
    public static int BoolToInt(bool value)
    {
        // true = 1, false = 0.
        return value ? 1 : 0;
    }

    public static bool IntToBool(int value)
    {
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
            return 0f;
        }
    }

    [Log]
    public static async Task<string> GetJsonFromUrl(string url, string tagName)
    {
        try
        {
            var apiResponse = await AppProperties.WebClient.GetAsync(url);
            if (!apiResponse.IsSuccessStatusCode) return "Error Getting Tag Information";

            var responseData = await apiResponse.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseData);
            return document.RootElement.TryGetProperty(tagName, out var tagInfo) 
                ? tagInfo.ToString() 
                : $"Error Getting Tag Information: {apiResponse.StatusCode}";
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

        return (forceBuildNumber || build != "0") 
            ? $"{major}.{minor}.{build}" 
            : $"{major}.{minor}";
    }
}
