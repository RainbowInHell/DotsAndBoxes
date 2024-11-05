namespace DotsAndBoxesServerAPI;

public static class HubEventActions
{
    private static readonly IReadOnlyDictionary<HubEventActionType, string> HubEventActionNames;

    static HubEventActions()
    {
        HubEventActionNames = new Dictionary<HubEventActionType, string>
        {
            { HubEventActionType.OnPlayerConnect, HubEventActionType.OnPlayerConnect.ToString() },
            { HubEventActionType.OnPlayerDisconnect, HubEventActionType.OnPlayerDisconnect.ToString() },
            { HubEventActionType.OnPlayerUpdateSettings, HubEventActionType.OnPlayerUpdateSettings.ToString() },
            { HubEventActionType.OnPlayerChangeStatus, HubEventActionType.OnPlayerChangeStatus.ToString() },

            { HubEventActionType.OnChallenge, HubEventActionType.OnChallenge.ToString() },
            { HubEventActionType.OnChallengeCancel, HubEventActionType.OnChallengeCancel.ToString() },
            { HubEventActionType.OnChallengeReject, HubEventActionType.OnChallengeReject.ToString() },
            { HubEventActionType.OnChallengeAccept, HubEventActionType.OnChallengeAccept.ToString() },

            { HubEventActionType.OnOpponentMakeMove, HubEventActionType.OnOpponentMakeMove.ToString() },
            { HubEventActionType.OnGainPoints, HubEventActionType.OnGainPoints.ToString() },
            { HubEventActionType.OnGameEnd, HubEventActionType.OnGameEnd.ToString() },
            { HubEventActionType.OnOpponentLeaveGame, HubEventActionType.OnOpponentLeaveGame.ToString() }
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