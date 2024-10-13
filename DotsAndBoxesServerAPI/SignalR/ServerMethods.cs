﻿namespace DotsAndBoxesServerAPI.SignalR;

public static class ServerMethods
{
    private static readonly Dictionary<ServerMethodType, string> ServerMethodNames;

    static ServerMethods()
    {
        ServerMethodNames = new Dictionary<ServerMethodType, string>
        {
            { ServerMethodType.NewPlayerConnect, ServerMethodType.NewPlayerConnect.ToString() },
            { ServerMethodType.PlayerUpdateSettings, ServerMethodType.PlayerUpdateSettings.ToString() },
            { ServerMethodType.PlayerDisconnect, ServerMethodType.PlayerDisconnect.ToString() },
            { ServerMethodType.PlayerSendChallenge, ServerMethodType.PlayerSendChallenge.ToString() },
            { ServerMethodType.PlayerCancelChallenge, ServerMethodType.PlayerCancelChallenge.ToString() },
            { ServerMethodType.PlayerSendChallengeAnswer, ServerMethodType.PlayerSendChallengeAnswer.ToString() }
        };
    }

    public static string GetServerMethodName(ServerMethodType serverMethodType)
    {
        ServerMethodNames.TryGetValue(serverMethodType, out var result);

        if (result == null)
        {
            throw new ArgumentException(nameof(result), $"The following Server Method does not exist with this key. {nameof(serverMethodType).ToUpper()}: {serverMethodType}");
        }

        return result;
    }
}