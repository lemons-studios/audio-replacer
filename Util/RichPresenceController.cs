using AudioReplacer.Generic;
using DiscordRPC;
namespace AudioReplacer.Util;

public class RichPresenceController
{
    private readonly string details, smallImage, smallImageText, state;
    private readonly DiscordRpcClient client;
    private readonly Timestamps startTimestamp;
    private readonly long clientId = 1325340097234866297; // Change this to use your own rich presence client

    public RichPresenceController(string initialDetails, string initialState, string initialSmallImage, string initialSmallImageText)
    {
        bool autoCreate = AppFunctions.IntToBool(App.AppSettings.EnableRichPresence);

        startTimestamp = Timestamps.Now;
        state = initialState;
        details = initialDetails;
        smallImage = initialSmallImage;
        smallImageText = initialSmallImageText;

        client = new DiscordRpcClient(clientId.ToString());
        if (autoCreate) CreateRichPresence();
    }

    [Log]
    public void CreateRichPresence()
    {
        client.Initialize();
        client.SetPresence(new RichPresence
        {
            State = state,
            Details = details,
            Timestamps = startTimestamp,
            Assets = new Assets
            {
                LargeImageKey = "appicon",
                LargeImageText = $"Version {AppFunctions.GetAppVersion()}",
                SmallImageKey = smallImage,
                SmallImageText = smallImageText
            }
        });
    }

    public void SetState(string x)
    {
        client.UpdateState(x);
    }

    public void SetDetails(string x)
    {
        client.UpdateDetails(x);
    }

    public void SetSmallImage(string x)
    {
        client.UpdateSmallAsset(key: x);
    }

    public void SetSmallImageText(string x)
    {
        client.UpdateSmallAsset(tooltip: x);
    }

    public void SetLargeImage(string x)
    {
        client.UpdateLargeAsset(key: x);
    }

    public void SetLargeImageText(string x)
    {
        client.UpdateLargeAsset(tooltip: x);
    }

    public void SetLargeAsset(string key, string tooltip)
    {
        SetLargeImage(key);
        SetLargeImageText(tooltip);
    }

    public void SetSmallAsset(string key, string tooltip)
    {
        SetSmallImage(key);
        SetSmallImageText(tooltip);
    }

    public void DisposeRpc()
    {
        client.Dispose();
    }
}
