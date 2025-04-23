using System.Diagnostics;
using Velopack;

namespace AudioReplacer.Util;
public static class AppUpdater
{
    // Velopack will search through the url below for updates
    // Specifically a json file on a remote BackBlaze server. The address to that server is located in MainWindow (because I want updated to be checked AFTEr the application launches)
#pragma warning disable CA2211
    public static UpdateManager AppUpdateManager;
#pragma warning restore CA2211
    private static UpdateInfo appUpdateInfo;

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
            await AppUpdateManager.DownloadUpdatesAsync(appUpdateInfo).ConfigureAwait(true);
            AppUpdateManager.ApplyUpdatesAndRestart(appUpdateInfo);
            
        }
        catch (Exception)
        {
            // ReSharper disable once RedundantJumpStatement
            return;
        }
    }

    public static async Task SearchForUpdates()
    {
        try
        {
            if (!Debugger.IsAttached && AppFunctions.IntToBool(App.AppSettings.AppUpdateCheck))
            {
                appUpdateInfo = await AppUpdateManager.CheckForUpdatesAsync().ConfigureAwait(true);
                if (appUpdateInfo != null)
                {
                    Broadcast();
                }
            }
        }
        catch (Exception)
        {
            return;
        }
    }
}
