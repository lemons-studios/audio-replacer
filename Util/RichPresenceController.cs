using AudioReplacer.Generic;
using DiscordRPC;
using System.Threading.Tasks;

namespace AudioReplacer.Util;

public class RichPresenceController
{
    public string details, smallImage, smallImageText, state;
    private readonly DiscordRpcClient client;
    private readonly Timestamps startTimestamp;

    public RichPresenceController(long clientId, string initialDetails, string initialState, string initialSmallImage, string initialSmallImageText)
    {
        startTimestamp = Timestamps.Now;
        state = initialState;
        details = initialDetails;
        smallImage = initialSmallImage;
        smallImageText = initialSmallImageText;

        client = new DiscordRpcClient(clientId.ToString());
        client.Initialize();

        Task.Run(async () =>
        {
            while (true)
            {
                UpdateRPCStatus();
                await Task.Delay(1000);
            }
        });
    }

    private void UpdateRPCStatus()
    {
        client.SetPresence(new RichPresence()
        {
            State = state,
            Details = details,
            Timestamps = startTimestamp,
            Assets = new Assets()
            {
                LargeImageKey = "appicon",
                LargeImageText = $"Version {AppGeneric.GetAppVersion(true)}",
                SmallImageKey = smallImage,
                SmallImageText = smallImageText
            }
        });
    }

    public void SetState(string x)
    {
        state = x;
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
        client.Dispose();
    }
}
