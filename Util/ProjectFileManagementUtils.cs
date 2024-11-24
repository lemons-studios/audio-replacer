using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace AudioReplacer2.Util
{
    public class ProjectFileManagementUtils
    {
        private string currentFile, truncatedCurrentFile, currentOutFile, currentFileName, directoryName;
        private readonly string outputFolderPath, setupIgnore, projectPath;

        public ProjectFileManagementUtils(string path)
        {
            outputFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Out";
            projectPath = path;
            setupIgnore = $"{outputFolderPath}\\.setupIgnore";

            CreateInitialData();
            SetCurrentFile();
        }

        private void SetCurrentFile()
        {
            FindDeleteEmptyDirs();

            // Refresh subdirectories after potential deletion
            string[] subdirectories = GetPathSubdirectories(projectPath);

            currentFile = GetFirstAudioFile(projectPath);
            truncatedCurrentFile = currentFile == "" ? "YOU ARE DONE!!!" : TruncateDirectory(currentFile, 2);

            currentOutFile = $"{outputFolderPath}\\{truncatedCurrentFile}";
            currentFileName = TruncateDirectory(currentFile, 1);
            directoryName = truncatedCurrentFile.Split("\\")[0];
        }

        private void CreateInitialData()
        {
            string[] inFolderStructure = GetPathSubdirectories(projectPath);
            if (!DoesDirectoryExist(outputFolderPath))
            {
                GetFilesInFolder(outputFolderPath);
            }

            if (inFolderStructure != GetPathSubdirectories(outputFolderPath) && !File.Exists(setupIgnore)) 
            {
                string[] subdirectoryNames = TruncateSubdirectories(inFolderStructure);
                for (int i = 0; i < inFolderStructure.Length; i++)
                {
                    CreateDirectory($"{outputFolderPath}\\{subdirectoryNames[i]}");
                }
                File.Create(setupIgnore);
            }
        }

        private string TruncateDirectory(string inputPath, int dirLevels, string delimiter = "\\")
        {
            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(delimiter) || dirLevels <= 0) return inputPath;

            StringBuilder truncatedDir = new StringBuilder();
            string[] splitDir = inputPath.Split(delimiter);
            Array.Reverse(splitDir);

            dirLevels = Math.Min(dirLevels, splitDir.Length);

            for (int i = dirLevels - 1; i >= 0; i--)
            {
                truncatedDir.Append(splitDir[i]);
                if (i != 0) truncatedDir.Append('\\');
            }
            return truncatedDir.ToString();
        }

        public int GetFileCount(string path)
        {
            int x = 0;
            string[] directories = GetPathSubdirectories(path);

            foreach (string dir in directories)
            {
                x += Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;
            }
            return x;
        }

        public void FindDeleteEmptyDirs()
        {
            foreach (var directory in Directory.GetDirectories(projectPath))
            {
                if (Directory.GetFiles(directory).Length == 0) Directory.Delete(directory);
            }
        }

        private string GetFirstAudioFile(string path)
        {
            string[] pathSubfiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (string projectFile in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                if (IsAudioFile(projectFile)) return projectFile;
            }
            return ""; // If no audio files are found, then return a blank path
        }

        public float CalculatePercentageComplete()
        {
            float inputFolderCount = GetFileCount(projectPath);
            float outputFolderCount = GetFileCount(outputFolderPath);
            return float.Round((outputFolderCount / (outputFolderCount + inputFolderCount)) * 100, 2);
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

        public string GetCurrentFile(bool truncated = true)
        {
            return truncated ? truncatedCurrentFile : currentFile;
        }

        public async Task<StorageFolder> GetDirectoryAsStorageFolder()
        {
            return await StorageFolder.GetFolderFromPathAsync($"{outputFolderPath}\\{directoryName}");
        }

        public string GetOutFilePath()
        {
            return currentOutFile;
        }

        public string GetProjectPath()
        {
            return projectPath;
        }

        public string GetCurrentFileName()
        {
            return currentFileName;
        }

        public void CreateDirectory(string dir)
        {
            Directory.CreateDirectory(dir);
        }

        public string[] GetFilesInFolder(string dir)
        {
            return Directory.GetFiles(dir);
        }

        public bool DoesDirectoryExist(string dir)
        {
            return Directory.Exists(dir);
        }

        private bool IsAudioFile(string path)
        {
            string[] supportedFileTypes = [".wav", ".mp3", ".aac", ".m4a", ".flac", ".ogg"]; // TODO: Find a full list of extensions that a MeidaPlayerElement Supports
            return supportedFileTypes.Any(fileType => path.EndsWith(fileType, StringComparison.OrdinalIgnoreCase));
        }

        private string[] GetPathSubdirectories(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
        }

        private string[] TruncateSubdirectories(string[] notTruncatedDirectories)
        {
            return notTruncatedDirectories.Select(dir => dir.Split(Path.DirectorySeparatorChar).Last()).ToArray();
        }
    }
}
