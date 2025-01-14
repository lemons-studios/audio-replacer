using AudioReplacer.Util;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace AudioReplacer.Windows.MainWindow.Util;

public static class ProjectFileUtils
{
    private static string currentFile, truncatedCurrentFile, currentOutFile, currentFileName, directoryName;
    private static string outputFolderPath, projectPath;
    private static string rootDataDirectoryPath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}";
    public static bool IsProjectLoaded = false;

    public delegate void BroadcastEventHandler();
    public static event BroadcastEventHandler OnProjectLoaded;

    public static void Broadcast()
    {
        OnProjectLoaded?.Invoke();
    }

    public static void SetProjectData(string path)
    {
        projectPath = path;
        outputFolderPath = @$"{rootDataDirectoryPath}\audio-replacer\out\{TruncateDirectory(path, 1)}";
        CreateInitialData();
        SetCurrentFile();
        IsProjectLoaded = true;
        Broadcast();
    }

    private static void SetCurrentFile()
    {
        FindDeleteEmptyDirs();
        currentFile = GetNextAudioFile(projectPath);
        truncatedCurrentFile = currentFile == "" ? "YOU ARE DONE!!!" : TruncateDirectory(currentFile, 2);

        currentOutFile = $"{outputFolderPath}\\{truncatedCurrentFile}";
        currentFileName = TruncateDirectory(currentFile, 1);
        directoryName = truncatedCurrentFile.Split("\\")[0];
    }

    private static void CreateInitialData()
    {
        string[] inFolderStructure = GetPathSubdirectories(projectPath);
        if (!DoesDirectoryExist(outputFolderPath)) CreateDirectory(outputFolderPath);
        if (inFolderStructure == GetPathSubdirectories(outputFolderPath) || File.Exists($"{outputFolderPath}\\.setupIgnore")) return;

        string[] subdirectoryNames = TruncateSubdirectories(inFolderStructure);
        for (int i = 0; i < inFolderStructure.Length; i++) { CreateDirectory($"{outputFolderPath}\\{subdirectoryNames[i]}"); }
        File.WriteAllText($"{outputFolderPath}\\.setupIgnore", "This file is here to tell AudioReplacer2 to ignore this folder when launching.\nDo not delete this unless starting a new project");
    }

    private static string TruncateDirectory(string inputPath, int dirLevels, string delimiter = "\\")
    {
        if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(delimiter) || dirLevels <= 0) return inputPath;

        string[] splitDir = inputPath.Split(delimiter);
        if (dirLevels > splitDir.Length) dirLevels = splitDir.Length;

        var truncatedDir = splitDir[^dirLevels..];
        return string.Join(delimiter, truncatedDir);
    }

    public static int GetFileCount(string path)
    {
        string[] directories = GetPathSubdirectories(path);
        return directories.Sum(dir => Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length);
    }

    private static string GetNextAudioFile(string path)
    {
        var audioFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(IsAudioFile).ToList();

        // If there are no audio files, return nothing
        if (!audioFiles.Any()) return string.Empty;

        // Pick a random audio file if input file randomization is enabled
        if (Generic.IntToBool(App.AppSettings.InputRandomizationEnabled))
        {
            var rng = new Random();
            int randomFileIndex = rng.Next(audioFiles.Count);
            return audioFiles[randomFileIndex];
        }

        // Return the first audio file (if input is not randomized)
        return audioFiles.First();
    }

    public static float CalculatePercentageComplete()
    {
        float inputFolderCount = GetFileCount(projectPath);
        float outputFolderCount = GetFileCount(outputFolderPath);
        return float.Round(outputFolderCount / (outputFolderCount + inputFolderCount) * 100, 2);
    }

    public static void SkipAudioTrack()
    {
        File.Move(currentFile, currentOutFile);
        SetCurrentFile();
    }

    public static void DeleteCurrentFile()
    {
        File.Delete(currentFile);
        SetCurrentFile();
    }

    private static bool IsAudioFile(string path)
    {
        string[] supportedFileTypes = [".mp3", ".wav", ".wma", ".aac", ".m4a", ".flac", ".ogg", ".amr", ".aiff", ".3gp", ".asf", ".pcm"]; // This should be a good enough list. Most people will have either wav or mp3 anyway (maybe flac or ogg but that's really it)
        return supportedFileTypes.Any(fileType => path.EndsWith(fileType, StringComparison.OrdinalIgnoreCase));
    }

    // Another go at my yummy code minification
    private static string[] GetPathSubdirectories(string path)
    {
        return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
    }

    private static string[] TruncateSubdirectories(string[] notTruncatedDirectories)
    {
        return notTruncatedDirectories.Select(dir => dir.Split(Path.DirectorySeparatorChar).Last()).ToArray();
    }

    public static string GetCurrentFile(bool truncated = true)
    {
        return truncated ? truncatedCurrentFile : currentFile;
    }

    public static async Task<StorageFolder> GetDirectoryAsStorageFolder()
    {
        return await StorageFolder.GetFolderFromPathAsync($"{outputFolderPath}\\{directoryName}");
    }

    public static string GetOutFolderStructure()
    {
        return @$"{outputFolderPath}\{directoryName}";
    }

    public static string GetRootFolderPath()
    {
        return rootDataDirectoryPath;
    }

    public static string GetOutFilePath()
    {
        return currentOutFile;
    }

    public static string GetProjectPath()
    {
        return projectPath;
    }

    public static string GetCurrentFileName()
    {
        return currentFileName;
    }

    private static void CreateDirectory(string dir)
    {
        Directory.CreateDirectory(dir);
    }

    private static bool DoesDirectoryExist(string dir)
    {
        return Directory.Exists(dir);
    }

    private static void FindDeleteEmptyDirs()
    {
        foreach (string directory in Directory.GetDirectories(projectPath))
        {
            if (Directory.GetFiles(directory).Length == 0)
                Directory.Delete(directory);
        }
    }
}

