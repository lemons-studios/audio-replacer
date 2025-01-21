using System.Threading.Tasks;
using Velopack;

namespace AudioReplacer.Util;

public static class AppUpdater
{
    private static readonly UpdateManager AppUpdateManager = new("https://audioreplacerupdates.blob.core.windows.net/updates");
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
