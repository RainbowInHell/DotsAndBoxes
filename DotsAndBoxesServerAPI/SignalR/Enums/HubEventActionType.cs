namespace DotsAndBoxesServerAPI;

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

    OnOpponentMakeMove,
    OnGainPoints,
    OnGameEnd,
    OnOpponentLeaveGame
}