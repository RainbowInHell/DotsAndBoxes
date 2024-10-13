namespace DotsAndBoxesServerAPI.SignalR
{
    public enum ServerMethodType
    {
        NewPlayerConnect,

        PlayerUpdateSettings,

        PlayerSendChallenge,

        PlayerCancelChallenge,

        PlayerSendChallengeAnswer,

        PlayerDisconnect
    }
}
