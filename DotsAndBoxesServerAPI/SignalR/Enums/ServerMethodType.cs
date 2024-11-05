namespace DotsAndBoxesServerAPI;

public enum ServerMethodType
{
    PlayerConnect,
    PlayerDisconnect,
    PlayerUpdateSettings,

    PlayerSendChallenge,
    PlayerCancelChallenge,
    PlayerSendChallengeAnswer,

    OpponentMakeMove,
    OpponentLeaveGame
}