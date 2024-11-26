using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace AudioReplacer.Util
{
    public class ProjectFileManagementUtils
    {
        private string currentFile, truncatedCurrentFile, currentOutFile, currentFileName, directoryName;
        private readonly string outputFolderPath, projectPath;

        public ProjectFileManagementUtils(string path)
        {
            outputFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Out";
            projectPath = path;
            CreateInitialData();
            SetCurrentFile();
        }

        private void SetCurrentFile()
        {
            FindDeleteEmptyDirs();
            currentFile = GetFirstAudioFile(projectPath);
            truncatedCurrentFile = currentFile == "" ? "YOU ARE DONE!!!" : TruncateDirectory(currentFile, 2);

            currentOutFile = $"{outputFolderPath}\\{truncatedCurrentFile}";
            currentFileName = TruncateDirectory(currentFile, 1);
            directoryName = truncatedCurrentFile.Split("\\")[0];
        }

        private void CreateInitialData()
        {
            string[] inFolderStructure = GetPathSubdirectories(projectPath);
            if (!DoesDirectoryExist(outputFolderPath)) CreateDirectory(outputFolderPath);
            if (inFolderStructure == GetPathSubdirectories(outputFolderPath) || File.Exists($"{outputFolderPath}\\.setupIgnore")) return;
            
            string[] subdirectoryNames = TruncateSubdirectories(inFolderStructure);
            for (int i = 0; i < inFolderStructure.Length; i++) { CreateDirectory($"{outputFolderPath}\\{subdirectoryNames[i]}"); }
            File.WriteAllText($"{outputFolderPath}\\.setupIgnore", "This file is here to tell AudioReplacer2 to ignore this folder when launching.\nDo not delete this unless starting a new project");
        }

        private string TruncateDirectory(string inputPath, int dirLevels, string delimiter = "\\")
        {
            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(delimiter) || dirLevels <= 0) return inputPath;
            StringBuilder truncatedDir = new StringBuilder();
            string[] splitDir = inputPath.Split(delimiter);
            Array.Reverse(splitDir);
            dirLevels = Math.Min(dirLevels, splitDir.Length);

            for (int i = dirLevels - 1; i >= 0; i--) { truncatedDir.Append(splitDir[i]); if (i != 0) truncatedDir.Append('\\'); }
            return truncatedDir.ToString();
        }

        public int GetFileCount(string path)
        {
            string[] directories = GetPathSubdirectories(path);
            return directories.Sum(dir => Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length);
        }


        private string GetFirstAudioFile(string path)
        {
            string[] pathSubfiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (string projectFile in pathSubfiles) { if (IsAudioFile(projectFile)) return projectFile; }
            return ""; // If no audio files are found, then return a blank path
        }

        public float CalculatePercentageComplete()
        {
            float inputFolderCount = GetFileCount(projectPath);
            float outputFolderCount = GetFileCount(outputFolderPath);
            return float.Round(outputFolderCount / (outputFolderCount + inputFolderCount) * 100, 2);
        }

        public void SkipAudioTrack()
        {
            File.Move(currentFile, currentOutFile);
            SetCurrentFile();
        }

        public void DeleteCurrentFile()
        {
            File.Delete(currentFile);
            SetCurrentFile();
        }

        private bool IsAudioFile(string path)
        {
            string[] supportedFileTypes = [".mp3", ".wav", ".wma", ".aac", ".m4a", ".flac", ".ogg", ".amr", ".aiff", ".3gp", ".asf", ".pcm"];
            return supportedFileTypes.Any(fileType => path.EndsWith(fileType, StringComparison.OrdinalIgnoreCase));
        }

        // Another go at my yummy code minification
        private string[] GetPathSubdirectories(string path) { return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly); }
        private string[] TruncateSubdirectories(string[] notTruncatedDirectories) { return notTruncatedDirectories.Select(dir => dir.Split(Path.DirectorySeparatorChar).Last()).ToArray(); }
        public string GetCurrentFile(bool truncated = true) { return truncated ? truncatedCurrentFile : currentFile; }
        public async Task<StorageFolder> GetDirectoryAsStorageFolder() { return await StorageFolder.GetFolderFromPathAsync($"{outputFolderPath}\\{directoryName}"); }
        public string GetOutFilePath() { return currentOutFile; }
        public string GetProjectPath() { return projectPath; }
        public string GetCurrentFileName() { return currentFileName; }
        private void CreateDirectory(string dir) { Directory.CreateDirectory(dir); }
        private bool DoesDirectoryExist(string dir) { return Directory.Exists(dir); }
        private void FindDeleteEmptyDirs() { foreach (string directory in Directory.GetDirectories(projectPath)) { if (Directory.GetFiles(directory).Length == 0) Directory.Delete(directory); } }
    }
}
