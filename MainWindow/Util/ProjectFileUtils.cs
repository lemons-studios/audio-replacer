using AudioReplacer.Generic;
using AudioReplacer.Util;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace AudioReplacer.MainWindow.Util;
public static class ProjectFileUtils
{
    private static string currentFile, truncatedCurrentFile, currentOutFile, currentFileName, directoryName, currentFileLocalPath;
    private static string outputFolderPath, projectPath;
    public static bool IsProjectLoaded, ExtraEditsFlagged = false;

    public delegate void BroadcastEventHandler();
    public static event BroadcastEventHandler OnProjectLoaded;

    public static void Broadcast()
    {
        OnProjectLoaded?.Invoke();
        IsProjectLoaded = true;
    }

    [Log]
    public static void SetProjectData(string path)
    {
        projectPath = path;
        var projectName = projectPath.Split("\\")[^1];
        outputFolderPath = Path.Combine(AppProperties.ExtraApplicationData, "out", projectName);
        CreateInitialData();
        Task.Run(ConvertAudioFiles);
        SetCurrentFile();
        Broadcast();
    }

    [Log]
    private static void SetCurrentFile()
    {
        currentFile = GetNextAudioFile(projectPath);

        if (string.IsNullOrEmpty(currentFile))
        {
            truncatedCurrentFile = "YOU ARE DONE!";
            currentOutFile = string.Empty;
            currentFileName = string.Empty;
            directoryName = string.Empty;
            return;
        }

        truncatedCurrentFile = TruncateDirectory(currentFile, 2);
        currentFileName = Path.GetFileName(currentFile);
        directoryName = TruncateDirectory(Path.GetDirectoryName(currentFile)!, 1);

        // This essentially gets the path to the file but removes the path from the root. useful for setting the output file
        // C:\path\to\project\then\file.wav will be split with C:\path\to\project\, creating an array with then\to\file at position 1

        // We can then combine the output directory (%appdata%\audio-replacer\out\[project-name]) with the split directory from the above variable
        // To get the absolute path to the output. This fixes an issue where audio-replacer only worked with files from only 2 subdirectories in
        currentFileLocalPath = currentFile.Split(projectPath)[1];
        currentOutFile = Path.Join(outputFolderPath, currentFileLocalPath);
    }

    // The application prefers that all input files are of the .wav format
    [Log]
    private static async Task ConvertAudioFiles()
    {
        try
        {
            var unconvertedFiles = GetAllFiles().Where(IsUndesirableAudioFile).ToList();
            if (unconvertedFiles.Count != 0)
            {
                bool isNotificationOpen = false;
                var totalFiles = unconvertedFiles.Count + 1;

                for (int i = 0; i < totalFiles - 1; i++)
                {

                    var input = unconvertedFiles[i];
                    var output = $"{input.Split(".")[0]}";
                    await AppFunctions.FfMpegCommand(unconvertedFiles[i], "-ar 1600 -b:a 16k -y", $"{output}.wav", true); // Force convert to .wav format}
                    File.Delete(input);

                    if (App.MainWindow != null)
                    {
                        float progress = MathF.Floor(((float) i / totalFiles) * 100);
                        if (!isNotificationOpen)
                        {
                            App.MainWindow.ToggleCompletionNotification("Converting files to .wav format...", $"Progress: {progress}%", progress);
                            isNotificationOpen = true;
                        }
                        else
                        {
                            App.MainWindow.SetCompletionMessage($"Progress: {progress}%", progress);
                        }
                    }
                }
                if(App.MainWindow != null)
                    App.MainWindow.ShowNotification(InfoBarSeverity.Success, "Success!", "All files converted");
            }
        }
        catch (Exception e)
        {
            if (App.MainWindow != null)
                App.MainWindow.ShowNotification(InfoBarSeverity.Error, "Error", e.Message);
        }
    }

    private static List<string> GetAllFiles()
    {
        return Directory.EnumerateFiles(projectPath, "*", SearchOption.AllDirectories).ToList();
    }

    private static void CreateInitialData()
    {
        if (!Directory.Exists(outputFolderPath))
            Directory.CreateDirectory(outputFolderPath);

        var inputDirectories = GetSubdirectories(projectPath);
        var outputDirectories = GetSubdirectories(outputFolderPath);

        inputDirectories = inputDirectories.Select(d => Path.GetFullPath(d).TrimEnd(Path.DirectorySeparatorChar)).ToArray();
        outputDirectories = outputDirectories.Select(d => Path.GetFullPath(d).TrimEnd(Path.DirectorySeparatorChar)).ToArray();

        if (inputDirectories.SequenceEqual(outputDirectories, StringComparer.OrdinalIgnoreCase))
            return;

        foreach (var dir in inputDirectories)
        {
            var relativePath = Path.GetRelativePath(projectPath, dir);
            var outputDir = Path.Combine(outputFolderPath, relativePath);

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
        }
    }

    private static string TruncateDirectory(string inputPath, int dirLevels, string delimiter = "\\")
    {
        if (string.IsNullOrEmpty(inputPath) || dirLevels <= 0) return inputPath;
        var splitDir = inputPath.Split(delimiter);

        if (dirLevels > splitDir.Length) dirLevels = splitDir.Length;
        return string.Join(delimiter, splitDir[^dirLevels..]);
    }

    private static string GetNextAudioFile(string path)
    {
        var audioFiles = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(IsAudioFile).ToList();
        if (!audioFiles.Any()) return string.Empty;

        if (AppFunctions.IntToBool(App.AppSettings.InputRandomizationEnabled))
        {
            // Pick random file from current project files if input randomization is enabled
            var rng = new Random();
            return audioFiles[rng.Next(audioFiles.Count)];
        }

        return audioFiles.First();
    }

    public static float CalculatePercentageComplete()
    {
        int inputFileCount = GetFileCount(projectPath);
        int outputFileCount = GetFileCount(outputFolderPath);
        return inputFileCount + outputFileCount == 0 ? 100 : (float) Math.Round(outputFileCount / (double) (inputFileCount + outputFileCount) * 100, 2);
    }
    public static void SkipAudioTrack()
    {
        if (!string.IsNullOrEmpty(currentFile) && !string.IsNullOrEmpty(currentOutFile))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(currentOutFile)!);
            File.Move(currentFile, currentOutFile);
            SetCurrentFile();
        }
    }

    [Log]
    public static void SubmitAudioFile()
    {
        if (!string.IsNullOrEmpty(currentFile))
        {
            File.Delete(currentFile);
        }

        if (ExtraEditsFlagged)
        {
            string dir = Path.GetDirectoryName(currentOutFile);
            string file = $"ExtraEditsRequired-{Path.GetFileName(currentOutFile)}";
            string joinedPath = Path.Join(dir, file);
            File.Move(currentOutFile!, joinedPath);
        }
        // Loop through all folders in input and delete any empty files
        foreach (var dir in Directory.GetDirectories(projectPath, "*", SearchOption.AllDirectories))
        {
            if (!Directory.EnumerateFileSystemEntries(dir).Any()) Directory.Delete(dir);
        }
        SetCurrentFile();
    }

    private static readonly string[] SupportedFileTypes = [".mp3", ".wav", ".wma", ".aac", ".m4a", ".flac", ".ogg", ".amr", ".aiff", ".3gp", ".asf", ".pcm"];
    private static bool IsAudioFile(string path)
    {
        return SupportedFileTypes.Any(fileType => path.EndsWith(fileType, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsUndesirableAudioFile(string path)
    {
        return SupportedFileTypes.Any(fileType => path.EndsWith(fileType, StringComparison.OrdinalIgnoreCase))
               && !path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);
    }

    private static string[] GetSubdirectories(string path)
    {
        return Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
    }

    public static string GetCurrentFile(bool truncate = true)
    {
        return truncate ? truncatedCurrentFile : currentFile;
    }

    public static string GetOutFilePath()
    {
        return currentOutFile;
    }

    public static string GetProjectPath()
    {
        return projectPath;
    }

    public static string GetCurrentFileName(bool humanizeOutput = false)
    {
        return humanizeOutput ? currentFileName.Replace(@"\", "/") : currentFileName;
    }

    public static int GetFileCount(string path)
    {
        return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Count();
    }

    public static async Task<StorageFolder> GetDirectoryAsStorageFolder()
    {
        return await StorageFolder.GetFolderFromPathAsync(Path.Join(outputFolderPath, directoryName));
    }
}
