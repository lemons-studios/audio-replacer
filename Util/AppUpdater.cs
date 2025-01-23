using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Velopack;

namespace AudioReplacer.Util;
public static class AppUpdater
{
    // Velopack will search through the url below for updates
    // Specifically a json file, link to said json file below:
    // https://updates.lemon-studios.ca/updates/releases.win.json
    public static UpdateManager AppUpdateManager;
    public static UpdateInfo AppUpdateInfo;

    public static async Task UpdateApplication()
    {
        try
        {
            await File.WriteAllTextAsync($"{Generic.ExtraApplicationData}\\log.txt", "Method called");
            AppUpdateInfo = await AppUpdateManager.CheckForUpdatesAsync().ConfigureAwait(true);
            if (AppUpdateInfo != null && Debugger.IsAttached == false)
            {
                await File.AppendAllTextAsync($"{Generic.ExtraApplicationData}\\log.txt", $"Update Found");
                await AppUpdateManager.DownloadUpdatesAsync(AppUpdateInfo).ConfigureAwait(true);
            }
            else
            {
                await File.AppendAllTextAsync($"{Generic.ExtraApplicationData}\\log.txt", $"No Update Found");
            }
        }
        catch (Exception e)
        {
            await File.AppendAllTextAsync($"{Generic.ExtraApplicationData}\\log.txt", $"Error: {e.Message}");
            throw new Exception(e.Message);
        }
    }

    public static async Task<bool> AreUpdatesAvailable()
    {
        return await AppUpdateManager.CheckForUpdatesAsync() != null;
    }
}
