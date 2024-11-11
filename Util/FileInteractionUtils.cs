using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace AudioReplacer2.Util
{
    public class FileInteractionUtils
    {
        private string currentFile, truncatedCurrentFile, currentOutFile, currentFileName, directoryName;
        private readonly string outputFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Out";
        private readonly string setupIgnore, projectPath;

        public FileInteractionUtils(string path)
        {
            projectPath = path;
            setupIgnore = $"{outputFolderPath}\\.setupIgnore";

            CreateInitialData();
            SetCurrentFile();
        }

        public void SetCurrentFile()
        {
            if (IsFirstDirEmpty())
            {
                // Delete the empty first folder
                Directory.Delete(GetPathSubdirectories(projectPath)[0], recursive: true);
            }

            // Refresh subdirectories after potential deletion
            string[] subdirectories = GetPathSubdirectories(projectPath);

            currentFile = subdirectories.Length > 0 ? GetFiles(subdirectories[0])[0] : "YOU ARE DONE!!!!!!";

            truncatedCurrentFile = currentFile == "YOU ARE DONE!!!!!!" ? currentFile : TruncateDirectory(currentFile, 2);
            currentOutFile = $"{outputFolderPath}\\{truncatedCurrentFile}";
            currentFileName = TruncateDirectory(currentFile, 1);
            directoryName = truncatedCurrentFile.Split("\\")[0];
        }

        private void CreateInitialData()
        {
            string[] inFolderStructure = GetPathSubdirectories(projectPath);
            if (!DoesDirExist(outputFolderPath))
            {
                CreateDirectory(outputFolderPath);
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

        private string[] GetPathSubdirectories(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
        }

        private string[] TruncateSubdirectories(string[] notTruncatedDirectories)
        {
            return notTruncatedDirectories.Select(dir => dir.Split(Path.DirectorySeparatorChar).Last()).ToArray();
        }

        public string TruncateDirectory(string inputPath, int dirLevels, string delimiter = "\\")
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

        public int GetFilesRemaining()
        {
            int x = 0;
            string[] directories = GetPathSubdirectories(projectPath);

            foreach (string dir in directories)
            {
                x += Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;
            }
            return x;
        }

        // Next 3 methods are Probably not needed, but I'd rather type out one method than a class then method
        private void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        private string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        private bool DoesDirExist(string path)
        {
            return Directory.Exists(path);
        }

        public string GetOutFilePath()
        {
            return currentOutFile;
        }

        public string GetCurrentFile(bool truncated = true)
        {
            return truncated ? truncatedCurrentFile : currentFile;
        }

        public async Task<StorageFolder> GetDirectoryAsStorageFolder()
        {
            return await StorageFolder.GetFolderFromPathAsync($"{outputFolderPath}\\{directoryName}");
        }

        public string GetCurrentFileName()
        {
            return currentFileName;
        }

        private bool IsFirstDirEmpty()
        {
            string[] subdirs = GetPathSubdirectories(projectPath);
            return subdirs.Length > 0 && Directory.GetFiles(subdirs[0]).Length == 0;
        }
    }
}
