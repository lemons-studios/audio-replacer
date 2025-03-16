using System.Diagnostics;
using Velopack;

namespace AudioReplacer.Util;
public static class AppUpdater
{
    // Velopack will search through the url below for updates
    // Specifically a json file on a remote backblaze server. The address to that server is located in MainWindow
    public static UpdateManager AppUpdateManager;
    private static UpdateInfo AppUpdateInfo;

    public delegate void BroadcastEventHandler();
    public static event BroadcastEventHandler OnUpdateFound;

    public static void Broadcast()
    {
        OnUpdateFound?.Invoke();
    }

    [Log]
    public static async Task UpdateApplication()
    {
        try
        {
            if (!Debugger.IsAttached && AppFunctions.IntToBool(App.AppSettings.AppUpdateCheck))
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
        catch (Exception)
        {
            return;
        }
    }
}
