using System.Threading.Tasks;
using Velopack;

namespace AudioReplacer.Util;

public static class AppUpdater
{
    // Velopack will search through the url below for updates
    private static readonly UpdateManager AppUpdateManager = new("https://updates.lemon-studios.ca/updates");
    
    public static async Task UpdateApplication()
    {
        var newVer = await AppUpdateManager.CheckForUpdatesAsync();
        if (newVer != null)
        {
            await AppUpdateManager.DownloadUpdatesAsync(newVer);
            AppUpdateManager.ApplyUpdatesAndRestart(newVer);
        }
    }

    public static async Task<bool> AreUpdatesAvailable()
    {
        var check = await AppUpdateManager.CheckForUpdatesAsync();
        return check != null;
    }
}
