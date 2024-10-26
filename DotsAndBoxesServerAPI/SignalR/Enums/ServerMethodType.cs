namespace DotsAndBoxesServerAPI.SignalR
{
    public enum ServerMethodType
    {
        PlayerConnect,
        PlayerDisconnect,
        PlayerUpdateSettings,

        PlayerSendChallenge,
        PlayerCancelChallenge,
        PlayerSendChallengeAnswer,

        PlayerMakeMove,
        PlayerEndGame
    }
}
