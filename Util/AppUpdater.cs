using System.Diagnostics;
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

    public delegate void BroadcastEventHandler();
    public static event BroadcastEventHandler OnUpdateFound;

    public static void Broadcast()
    {
        OnUpdateFound?.Invoke();
    }

    public static async Task UpdateApplication()
    {
        try
        {
            if (!Debugger.IsAttached && Generic.IntToBool(App.AppSettings.AppUpdateCheck))
            {
                AppUpdateInfo = await AppUpdateManager.CheckForUpdatesAsync().ConfigureAwait(true);
                if (AppUpdateInfo != null)
                {
                    Broadcast();
                    await AppUpdateManager.DownloadUpdatesAsync(AppUpdateInfo).ConfigureAwait(true);
                    AppUpdateManager.ApplyUpdatesAndRestart(AppUpdateInfo);
                }
            }
        }
        catch
        {
            return;
        }
    }
}
