using System.Diagnostics;

namespace AudioReplacer.Util
{
    public static class ShellCommandManager
    {
        public static Process CreateProcess(string command, string arguments, bool shellExecute = false, bool redirectOutput = true, bool redirectError = true, bool createWindow = false)
        {
            return new Process
            {
                StartInfo =
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = shellExecute,
                    RedirectStandardOutput = redirectOutput,
                    RedirectStandardError = redirectError,
                    CreateNoWindow = !createWindow 
                }
            };
        }
    }
}
