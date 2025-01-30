using System.IO;

namespace AudioReplacer.Util;
public static class AppLogger
{
    private static readonly string LogFilePath = Path.Join(Generic.LoggerPath, Generic.LoggerFileName);
    public static void LogEvent(string logContent)
    {
        if (!File.Exists(LogFilePath)) File.Create(LogFilePath);
        File.AppendAllText(LogFilePath, string.IsNullOrWhiteSpace(File.ReadAllText(LogFilePath)) ? $"\n{logContent}" : logContent);
    }
}
