using AudioReplacer.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;

namespace AudioReplacer.Windows.MainWindow.Util;
public static class ProjectFileUtils
{
    private static string currentFile, truncatedCurrentFile, currentOutFile, currentFileName, directoryName;
    private static string outputFolderPath, projectPath;
    public static bool IsProjectLoaded;

    public delegate void BroadcastEventHandler();
    public static event BroadcastEventHandler OnProjectLoaded;

    public static void Broadcast()
    {
        OnProjectLoaded?.Invoke();
        IsProjectLoaded = true;
    }

    public static void SetProjectData(string path)
    {
        projectPath = path;
        outputFolderPath = Path.Join(Generic.ExtraApplicationData, "out", TruncateDirectory(path, 1));
        CreateInitialData();
        SetCurrentFile();
        Broadcast();
    }

    private static void SetCurrentFile()
    {
        FindAndDeleteEmptyDirs();
        currentFile = GetNextAudioFile(projectPath);

        if (string.IsNullOrEmpty(currentFile))
        {
            truncatedCurrentFile = "YOU ARE DONE!";
            currentOutFile = string.Empty;
            currentFileName = string.Empty;
            directoryName = string.Empty;
        }
        else
        {
            truncatedCurrentFile = TruncateDirectory(currentFile, 2);
            currentOutFile = Path.Join(outputFolderPath, truncatedCurrentFile);
            currentFileName = Path.GetFileName(currentFile);
            directoryName = TruncateDirectory(Path.GetDirectoryName(currentFile)!, 1);
        }
    }

    // The application prefers that all input files are of the .wav format
    private static async Task ConvertAudioFiles()
    {
        List<string> projectFiles = GetAllFiles().Where(IsUndesirableAudioFile).ToList();
        if (projectFiles.Count != 0)
        {
            int totalFiles = projectFiles.Count + 1;
            App.MainWindow.ToggleProgressNotification("Converting files to .Wav format...", "Progress: 0%");
            for (int i = 0; i < totalFiles - 1; i++)
            {
                var input = projectFiles[i];
                var output = $"{input.Split(".")[0]}.wav";
                await Generic.SpawnProcess("ffmpeg", $"-i {projectFiles[i]} -ar 16000 -b:a 16k {output}");

                File.Delete(input);
                App.MainWindow.SetProgressMessage($"Progress: {MathF.Floor(((float) i / totalFiles) * 100)}%");
            }
            App.MainWindow.ShowNotification(InfoBarSeverity.Success, "Success!", "All files converted");
            return;
        }
        
        App.MainWindow.ShowNotification(InfoBarSeverity.Informational, "File Conversion Skipped",
            "All files are of preferred type, auto-conversion has been turned off in settings or no valid input files have been found");
    }

    private static List<string> GetAllFiles()
    {
        return Directory.EnumerateFiles(projectPath, "*" ,SearchOption.AllDirectories).ToList();
    }

    private static void CreateInitialData()
    {
        var setupIgnorePath = Path.Combine(outputFolderPath, ".setupIgnore");

        if (!Directory.Exists(outputFolderPath))
            Directory.CreateDirectory(outputFolderPath);

        var inputDirectories = GetPathSubdirectories(projectPath);
        var outputDirectories = GetPathSubdirectories(outputFolderPath);

        inputDirectories = inputDirectories.Select(d => Path.GetFullPath(d).TrimEnd(Path.DirectorySeparatorChar)).ToArray();
        outputDirectories = outputDirectories.Select(d => Path.GetFullPath(d).TrimEnd(Path.DirectorySeparatorChar)).ToArray();

        if (inputDirectories.SequenceEqual(outputDirectories, StringComparer.OrdinalIgnoreCase) || File.Exists(setupIgnorePath))
           return;

        foreach (var dir in inputDirectories)
        {
            var relativePath = Path.GetRelativePath(projectPath, dir);
            var outputDir = Path.Combine(outputFolderPath, relativePath);

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
        }

        // Create the ignore file
        File.WriteAllText(setupIgnorePath, "This file is here to tell Audio Replacer to ignore this folder when launching. Do NOT delete");
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

        if (Generic.IntToBool(App.AppSettings.InputRandomizationEnabled))
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
        return inputFileCount + outputFileCount == 0 ? 100 : (float) Math.Round((outputFileCount / (double) (inputFileCount + outputFileCount)) * 100, 2);
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

    public static void DeleteCurrentFile()
    {
        if (!string.IsNullOrEmpty(currentFile))
        {
            File.Delete(currentFile);
            SetCurrentFile();
        }
    }

    private static void FindAndDeleteEmptyDirs()
    {
        foreach (var dir in Directory.GetDirectories(projectPath, "*", SearchOption.AllDirectories))
        {
            if (!Directory.EnumerateFileSystemEntries(dir).Any())
                Directory.Delete(dir);
        }
    }

    private static bool IsAudioFile(string path)
    {
        // I should PROBABLY rewrite this method in a way that includes all audio file types, but this list already contains the popular audio formats
        string[] supportedFileTypes = [ ".mp3", ".wav", ".wma", ".aac", ".m4a", ".flac", ".ogg", ".amr", ".aiff", ".3gp", ".asf", ".pcm" ];
        return supportedFileTypes.Any(fileType => path.EndsWith(fileType, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsUndesirableAudioFile(string path)
    {
        // I am a GENIUS
        string[] supportedFileTypes = [".mp3", ".wma", ".aac", ".m4a", ".flac", ".ogg", ".amr", ".aiff", ".3gp", ".asf", ".pcm"];
        return supportedFileTypes.Any(fileType => path.EndsWith(fileType, StringComparison.OrdinalIgnoreCase));
    }

    private static string[] GetPathSubdirectories(string path)
    {
        return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
    }

    public static string GetCurrentFile(bool truncated = true)
    {
        return truncated ? truncatedCurrentFile : currentFile;
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
