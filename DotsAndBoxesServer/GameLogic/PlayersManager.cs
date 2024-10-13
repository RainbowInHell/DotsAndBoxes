using System.Collections.Concurrent;
using DotsAndBoxesServerAPI.Models;

namespace DotsAndBoxesServer.GameLogic;

public class PlayersManager
{
    private readonly ConcurrentDictionary<string, Player> _connectionIdToPlayer = new();

    private readonly ConcurrentDictionary<string, GameGroup> _groups = new();

    public void AddPlayer(string connectionId, Player player)
    {
        if (!_connectionIdToPlayer.TryAdd(connectionId, player))
        {
            throw new ArgumentException($"Can't add player with {connectionId} connection id", nameof(connectionId));
        }
    }

    public Player RemovePlayer(string connectionId)
    {
        if (!_connectionIdToPlayer.TryRemove(connectionId, out var disconnectedPlayer))
        {
            throw new KeyNotFoundException($"Player with connection id '{connectionId}' not found");
        }

        // If disconnected player was group host - delete this group.
        _groups.TryRemove(connectionId, out _);

        return disconnectedPlayer;
    }

    public IEnumerable<Player> GetConnectedPlayers()
    {
        return _connectionIdToPlayer.Values;
    }

    public IEnumerable<Player> GetConnectedPlayers(string ignoredConnectionId)
    { 
        return _connectionIdToPlayer.Where(kvp => kvp.Key != ignoredConnectionId).Select(kvp => kvp.Value);
    }

    public Player GetConnectedPlayer(string connectionId)
    {
        if (_connectionIdToPlayer.TryGetValue(connectionId, out var player))
        {
            return player;
        }

        throw new KeyNotFoundException($"Player with connection id '{connectionId}' not found");
    }

    public string GetPlayerConnectionId(string playerName)
    {
        return _connectionIdToPlayer.FirstOrDefault(entry => entry.Value.Name == playerName).Key;
    }

    public Player UpdatePlayer(string connectionId, SettingsHolder newSettings)
    {
        var playerToUpdate = GetConnectedPlayer(connectionId);
        playerToUpdate.Status = newSettings.DoNotDisturb
                                    ? PlayerStatus.DoNotDisturb
                                    : PlayerStatus.FreeToPlay;
        playerToUpdate.Settings = newSettings;
        return playerToUpdate;
    }

    public GameGroup CreateGroup(string hostConnectionId)
    {
        var newGroup = new GameGroup();
        newGroup.AddPlayer(GetConnectedPlayer(hostConnectionId));
        _groups.TryAdd(hostConnectionId, newGroup);
        return newGroup;
    }

    public void AddToGroup(string hostConnectionId, string secondPlayerConnectionId)
    {
        var existedGroup = _groups[hostConnectionId];
        existedGroup.AddPlayer(GetConnectedPlayer(secondPlayerConnectionId));
    }

    public GameGroup DeleteGroup(string hostConnectionId)
    {
        if (_groups.TryRemove(hostConnectionId, out var deletedGroup))
        {
            return deletedGroup;
        }

        throw new KeyNotFoundException($"Group with host player connection id '{hostConnectionId}' not found");
    }
}