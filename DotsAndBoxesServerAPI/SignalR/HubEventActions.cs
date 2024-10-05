namespace DotsAndBoxesServerAPI.SignalR;

public static class HubEventActions
{
    private static readonly IReadOnlyDictionary<HubEventActionType, string> HubEventActionNames;

    static HubEventActions()
    {
        HubEventActionNames = new Dictionary<HubEventActionType, string>
        {
            { HubEventActionType.OnNewPlayerConnected, HubEventActionType.OnNewPlayerConnected.ToString() },
            { HubEventActionType.OnConnectedPlayersActualization, HubEventActionType.OnConnectedPlayersActualization.ToString() }
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