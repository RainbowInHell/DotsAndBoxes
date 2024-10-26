namespace DotsAndBoxesServerAPI.SignalR
{
    public enum HubEventActionType
    {
        OnPlayerConnect,
        OnPlayerDisconnect,
        OnPlayerUpdateSettings,
        OnPlayerChangeStatus,

        OnChallenge,
        OnChallengeCancel,
        OnChallengeReject,
        OnChallengeAccept,

        OnPlayerMakeMove
    }
}