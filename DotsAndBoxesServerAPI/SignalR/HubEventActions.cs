namespace DotsAndBoxesServerAPI.SignalR;

public static class HubEventActions
{
    private static readonly IReadOnlyDictionary<HubEventActionType, string> HubEventActionNames;

    static HubEventActions()
    {
        HubEventActionNames = new Dictionary<HubEventActionType, string>
        {
            { HubEventActionType.OnNewPlayerConnect, HubEventActionType.OnNewPlayerConnect.ToString() },
            { HubEventActionType.OnPlayerUpdateSettings, HubEventActionType.OnPlayerUpdateSettings.ToString() },
            { HubEventActionType.OnPlayerDisconnect, HubEventActionType.OnPlayerDisconnect.ToString() },
            { HubEventActionType.OnPlayerSendChallenge, HubEventActionType.OnPlayerSendChallenge.ToString() },
            { HubEventActionType.OnPlayerReceiveChallenge, HubEventActionType.OnPlayerReceiveChallenge.ToString() },
            { HubEventActionType.OnPlayerCancelChallenge, HubEventActionType.OnPlayerCancelChallenge.ToString() },
            { HubEventActionType.OnPlayerRejectChallenge, HubEventActionType.OnPlayerRejectChallenge.ToString() },
            { HubEventActionType.OnPlayerAcceptChallenge, HubEventActionType.OnPlayerAcceptChallenge.ToString() }
        };
    }

    public static string GetHubEventActionName(HubEventActionType hubEventAction)
    {
        HubEventActionNames.TryGetValue(hubEventAction, out var result);

        if (result == null)
        {
            throw new ArgumentException(nameof(result), $"The following Hub Event Action does not exist with this key. {nameof(hubEventAction).ToUpper()}: {hubEventAction}");
        }

        return result;
    }
}