using System.Collections.Concurrent;
using DotsAndBoxesServerAPI.Models;

namespace DotsAndBoxesServer;

public class PlayersManager
{
    private readonly ConcurrentDictionary<string, Player> _connectionIdToPlayer = new();

    private readonly ConcurrentDictionary<string, string> _playerToOpponent = new();

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

        return disconnectedPlayer;
    }

    public IEnumerable<Player> GetConnectedPlayers()
    {
        return _connectionIdToPlayer.Values;
    }

    public Player GetConnectedPlayer(string connectionId)
    {
        if (_connectionIdToPlayer.TryGetValue(connectionId, out var player))
        {
            return player;
        }

        throw new KeyNotFoundException($"Player with connection id '{connectionId}' not found");
    }

    public string GetConnectionId(string playerName)
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

    public void MapOpponents(string firstPlayerConnectionId, string secondPlayerConnectionId)
    {
        _playerToOpponent[firstPlayerConnectionId] = secondPlayerConnectionId;
        _playerToOpponent[secondPlayerConnectionId] = firstPlayerConnectionId;
    }

    public string GetOpponentConnectionId(string connectionId)
    {
        return _playerToOpponent.GetValueOrDefault(connectionId);
    }

    public void RemoveOpponent(string connectionId)
    {
        if (!_playerToOpponent.TryRemove(connectionId, out _))
        {
            throw new KeyNotFoundException($"Opponent for '{connectionId}' connection id not found");
        }
    }
}