using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Windows.Storage;

namespace AudioReplacer.MainWindow.Util;

/// <summary>
/// Manages paths to files in an AudioReplacer project
/// </summary>

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class ProjectFileUtils
{
    private static string currentFile, truncatedCurrentFile, currentOutFile, currentFileName, currentFileLocalPath;
    private static string outputFolderPath, projectPath;

    private static readonly string[] SupportedFileTypes = [".mp3", ".wav", ".wma", ".aac", ".m4a", ".flac", ".ogg", ".aiff"]; // TODO: Switch to some way to check for all audio file types
    public static bool IsProjectLoaded, ExtraEditsFlagged = false;

    public delegate void BroadcastEventHandler();
    public static event BroadcastEventHandler OnProjectLoaded;

    private static List<string> projectFiles;

    // ReSharper disable once MemberCanBePrivate.Global
    public static void Broadcast()
    {
        OnProjectLoaded?.Invoke();
        IsProjectLoaded = true;
    }

    [Log]
    public static void SetProjectData(string path)
    {
        projectPath = path;
        var projectName = projectPath.Split(Path.DirectorySeparatorChar)[^1];

        // I'm trying my best to maKe a LINQ query that sorts subdirectories first. Doesn't work at the moment :(
        projectFiles = GetAllFiles()
            .Where(IsAudioFile)
            .OrderBy(Path.GetDirectoryName)
            .ThenBy(Path.GetFileName)
            .ThenBy(p => p.Split(Path.DirectorySeparatorChar).Length)
            .ToList();

        outputFolderPath = Path.Join(AppProperties.OutputPath, projectName);
        if (!Directory.Exists(outputFolderPath))
            Directory.CreateDirectory(outputFolderPath);
        CreateInitialData();
        SetCurrentFile();

        Broadcast();
    }

    [Log]
    private static void SetCurrentFile()
    {
        currentFile = GetNextAudioFile();

        if (string.IsNullOrEmpty(currentFile))
        {
            truncatedCurrentFile = "YOU ARE DONE!";
            currentOutFile = string.Empty;
            currentFileName = string.Empty;
            return;
        }

        truncatedCurrentFile = TruncateDirectory(currentFile, 2);
        currentFileName = Path.GetFileName(currentFile);

        // Path manipulation trickery
        // tldr: split the path to the current file with the path to the project folder, then combine the split path with the path to the output folder
        currentFileLocalPath = currentFile.Split(projectPath)[1];
        currentOutFile = Path.Join(outputFolderPath, currentFileLocalPath);
    }

    [Log]
    private static async Task ConvertAudioFiles()
    {
        try
        {
            var unconvertedFiles = GetAllFiles().Where(IsUndesirableAudioFile).ToList();
            if (unconvertedFiles.Count != 0)
            {
                var isNotificationOpen = false;
                var totalFiles = unconvertedFiles.Count + 1;

                for (int i = 0; i < totalFiles - 1; i++)
                {

                    var input = unconvertedFiles[i];
                    var output = $"{input.Split(".")[0]}";
                    if (File.Exists(unconvertedFiles[i])) // Just in case....
                        await AppFunctions.FfMpegCommand(unconvertedFiles[i], "-y", $"{output}.wav"); // Force convert to .wav format

                    File.Delete(input);
                    if (App.MainWindow != null)
                    {
                        var percentage = MathF.Floor(((float) i / totalFiles) * 100);
                        if (!isNotificationOpen)
                        {
                            App.MainWindow.ToggleCompletionNotification("Converting files to .wav format", $"Progress: {percentage}%", percentage);
                            isNotificationOpen = true;
                        }
                        else
                        {
                            App.MainWindow.SetCompletionMessage($"Progress: {percentage}%", percentage);
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

    private static void CreateInitialData()
    {
        var inputDirectories = GetSubdirectories(projectPath).Select(d => Path.GetFullPath(d).TrimEnd(Path.DirectorySeparatorChar)).ToArray();
        var outputDirectories = GetSubdirectories(outputFolderPath).Select(d => Path.GetFullPath(d).TrimEnd(Path.DirectorySeparatorChar)).ToArray();

        if (inputDirectories.SequenceEqual(outputDirectories, StringComparer.OrdinalIgnoreCase))
            return;

        // Create a mirrored directory layout in the output folder
        foreach (var d in inputDirectories)
        {
            var relativePath = Path.GetRelativePath(projectPath, d);
            var outputDir = Path.Combine(outputFolderPath, relativePath);

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
        }

        Task.Run(ConvertAudioFiles);
    }
    
    private static string TruncateDirectory(string inputPath, int dirLevels, string delimiter = "\\")
    {
        if (string.IsNullOrEmpty(inputPath) || dirLevels <= 0) return inputPath;
        var splitDir = inputPath.Split(delimiter);

        if (dirLevels > splitDir.Length) dirLevels = splitDir.Length;
        return string.Join(delimiter, splitDir[^dirLevels..]);
    }

    private static readonly Random Rng = new();
    [Log]
    private static string GetNextAudioFile()
    {
        // This array should hopefully order by directory name and make subdirectories of directories appear with them
        // So a/b/c.wav will come before a/c2.wav

        try
        {
            // Incredible double ternary expression. reduced line count in this method by maybe 10-15
            return projectFiles.Any()
                ? AppFunctions.IntToBool(App.AppSettings.InputRandomizationEnabled)
                    ? projectFiles[Rng.Next(projectFiles.Count)]
                    : projectFiles.First()
                : string.Empty;
        }
        catch (Exception) // If anything goes wrong, return an empty string
        {
            return string.Empty; 
        }
    }

    /// <summary>Calculates how many files the user has completed in the current project</summary>
    /// <returns>Project Completion percentage</returns>
    public static float CalculatePercentageComplete()
    {
        var inCount = GetFileCount(projectPath);
        var outCount = GetFileCount(outputFolderPath);
        return inCount + outCount == 0 
            ? 100 // Edge case
            : (float) Math.Round(outCount / (double) (inCount + outCount) * 100, 2);
    }

    public static void SkipAudioTrack()
    {
        if (string.IsNullOrEmpty(currentFile) || string.IsNullOrEmpty(currentOutFile)) 
            return;
        
        File.Move(currentFile, currentOutFile);
        projectFiles.RemoveAt(0);
        SetCurrentFile();
    }

    [Log]
    public static void SubmitAudioFile()
    {
        if (!string.IsNullOrEmpty(currentFile)) 
            File.Delete(currentFile);
        
        if (ExtraEditsFlagged)
        {
            var dir = Path.GetDirectoryName(currentOutFile);
            var file = $"ExtraEditsRequired-{Path.GetFileName(currentOutFile)}";
            var joinedPath = Path.Join(dir, file);
            File.Move(currentOutFile!, joinedPath);
        }
        // Loop through all folders in input and delete any empty files
        foreach (var dir in Directory.GetDirectories(projectPath, "*", SearchOption.AllDirectories))
        {
            if (!Directory.EnumerateFileSystemEntries(dir).Any()) 
                Directory.Delete(dir);
        }
        projectFiles.RemoveAt(0);
        SetCurrentFile();
    }

    private static bool IsAudioFile(string path)
    {
        return SupportedFileTypes.Any(fileType => path.EndsWith(fileType, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsUndesirableAudioFile(string path)
    {
        return SupportedFileTypes.Any(fileType => path.EndsWith(fileType, StringComparison.OrdinalIgnoreCase)) && !path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);
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

    [Log]
    public static async Task<StorageFolder> GetDirectoryAsStorageFolder()
    {
        return await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(currentOutFile));
    } 
    
    private static List<string> GetAllFiles()
    {
        return Directory.EnumerateFiles(projectPath, "*", SearchOption.AllDirectories).ToList();
    }
}
