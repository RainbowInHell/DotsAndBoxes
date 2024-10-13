namespace DotsAndBoxesServerAPI.SignalR
{
    public enum HubEventActionType
    {
        OnNewPlayerConnect,

        OnPlayerUpdateSettings,

        OnPlayerSendChallenge,

        OnPlayerReceiveChallenge,

        OnPlayerCancelChallenge,

        OnPlayerRejectChallenge,

        OnPlayerAcceptChallenge,

        OnPlayerDisconnect
    }
}