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
        private string projectPath, currentFile, truncatedCurrentFile, currentOutFile, currentFileName, directoryName;
        private string outputFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AudioReplacer2-Out";

        public FileInteractionUtils(string path)
        {
            projectPath = path;
            CreateInitialData();
            SetCurrentFile();
        }

        public void SetCurrentFile()
        {
            if (isFirstDirEmpty())
            {
                // Delete the empty first folder
                Directory.Delete(getFolderSubdirs(projectPath)[0], recursive: true);
            }

            // Refresh subdirectories after potential deletion
            string[] subdirectories = getFolderSubdirs(projectPath);

            currentFile = subdirectories.Length > 0 && Directory.GetFiles(subdirectories[0]).Length > 0
                ? Directory.GetFiles(subdirectories[0])[0]
                : "YOU ARE DONE!!!!!!";

            truncatedCurrentFile = currentFile == "YOU ARE DONE!!!!!!" ? currentFile : truncateDirectory(currentFile, 2);
            currentOutFile = $"{outputFolderPath}\\{truncatedCurrentFile}";
            currentFileName = truncateDirectory(currentFile, 1);
            directoryName = truncatedCurrentFile.Split("\\")[0];
        }

        private void CreateInitialData()
        {
            string[] inFolderStructure = getFolderSubdirs(projectPath);
            string setupIgnore = $"{outputFolderPath}\\.setupIgnore";
            if (!Directory.Exists(outputFolderPath)) Directory.CreateDirectory(outputFolderPath);

            if (inFolderStructure != getFolderSubdirs(outputFolderPath) && !File.Exists(setupIgnore)) 
            {
                string[] subdirectoryNames = truncateSubdirs(inFolderStructure);
                for (int i = 0; i < inFolderStructure.Length; i++)
                {
                    Directory.CreateDirectory($"{outputFolderPath}\\{subdirectoryNames[i]}");
                }

                File.Create(setupIgnore);
            }
        }

        private string[] getFolderSubdirs(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
        }

        private string[] truncateSubdirs(string[] unsplicedSubdir)
        {
            return unsplicedSubdir.Select(dir => dir.Split(Path.DirectorySeparatorChar).Last()).ToArray();
        }

        public string truncateDirectory(string inputPath, int dirLevels, string delimiter = "\\")
        {
            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(delimiter) || dirLevels <= 0) return inputPath;

            StringBuilder truncatedDir = new StringBuilder();
            string[] splitDir = inputPath.Split(delimiter); 
            Array.Reverse(splitDir);

            dirLevels = Math.Min(dirLevels, splitDir.Length);

            for (int i = dirLevels - 1; i >= 0; i--)
            {
                truncatedDir.Append(splitDir[i]);
                if (i != 0) truncatedDir.Append("\\");
            }

            return truncatedDir.ToString();
        }

        public void SkipAudioTrack()
        {
            string outPath = $"{outputFolderPath}\\{truncateDirectory(currentFile, 2)}";
            File.Move(currentFile, outPath);
            SetCurrentFile();
        }

        public void DeleteCurrentFile()
        {
            File.Delete(currentFile);
            SetCurrentFile();
        }

        public async Task<StorageFolder> GetDirNameAsStorageFolder()
        {
            return await StorageFolder.GetFolderFromPathAsync($"{outputFolderPath}\\{directoryName}");
        }

        public string GetCurrentFile(bool truncated = true)
        {
            return truncated ? truncatedCurrentFile : currentFile;
        }

        public int GetFilesRemaining()
        {
            int count = 0;
            string[] subdirectories = getFolderSubdirs(projectPath);
            for (int i = 0; i < subdirectories.Length; i++)
            {
                count += Directory.GetFiles(subdirectories[i], "*", SearchOption.AllDirectories).Length;
            }
            return count;
        }

        public string GetOutFolderFile()
        {
            return currentOutFile;
        }

        public string GetCurrentFileName()
        {
            return currentFileName;
        }

        private bool isFirstDirEmpty()
        {
            string[] subdirs = getFolderSubdirs(projectPath);
            return subdirs.Length > 0 && Directory.GetFiles(subdirs[0]).Length == 0;
        }
    }
}
