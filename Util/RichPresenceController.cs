using AudioReplacer.Generic;
using DiscordGameSDKWrapper;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace AudioReplacer.Util;

public class RichPresenceController
{
    public string details, smallImage, smallImageText;
    private Discord discordRpc;
    private ActivityManager activityManager;
    private long startTime;

    public RichPresenceController(string initialDetails, string initialSmallImage, string initialSmallImageText)
    {
        discordRpc = new Discord(AppGeneric.richPresenceClientId, (UInt64)CreateFlags.NoRequireDiscord);
        activityManager = discordRpc.GetActivityManager();
        startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        details = initialDetails;
        smallImage = initialSmallImage;
        smallImageText = initialSmallImageText;

        Task.Run(() => SetRPCStatus());
    }

    private async Task SetRPCStatus()
    {
        while(true)
        {
            if (App.AppSettings.EnableRichPresence == 1)
            {
                try
                {
                    var activity = new Activity
                    {
                        State = "Using Audio Replacer",
                        Details = details,
                        Assets =
                        {
                            LargeImage = "appIcon",
                            SmallImage = smallImage,
                            LargeText = $"Version {AppGeneric.GetAppVersion(true)}",
                            SmallText = smallImageText
                        },
                        Timestamps =
                        {
                            Start = startTime
                        }
                    };

                    activityManager.UpdateActivity(activity, (res) =>
                    {
                        if (res == Result.Ok)
                        {

                        }
                    });
                }
                catch
                {
                    // Loop back again lol
                }
            }
            await Task.Delay(1);
        }
    }

    public void SetDetails(string x)
    {
        details = x;
    }

    public void SetSmallImage(string x)
    {
        smallImage = x;
    }

    public void SetSmallImageText(string x)
    {
        smallImageText = x;
    }

    public void DisposeRPC()
    {
        discordRpc.Dispose();
    }
}
