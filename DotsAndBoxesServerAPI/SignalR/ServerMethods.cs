namespace DotsAndBoxesServerAPI.SignalR;

public static class ServerMethods
{
    private static readonly IReadOnlyDictionary<ServerMethodType, string> ServerMethodNames;

    static ServerMethods()
    {
        ServerMethodNames = new Dictionary<ServerMethodType, string>
        {
            { ServerMethodType.NewPlayerConnected, ServerMethodType.NewPlayerConnected.ToString() },
            { ServerMethodType.ConnectedPlayersActualization, ServerMethodType.ConnectedPlayersActualization.ToString() },
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