using System.Collections.Concurrent;
using DotsAndBoxesServerAPI;

namespace DotsAndBoxesServer;

public class GameManager
{
    private readonly ConcurrentDictionary<string, Player> _connectionIdToPlayer = new();

    private readonly List<GameLobby> _gameLobbies = [];

    public void AddPlayer(string connectionId, Player player)
    {
        if (!_connectionIdToPlayer.TryAdd(connectionId, player))
        {
            throw new ArgumentException($"Can't add player with {connectionId} connection id", nameof(connectionId));
        }
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

    public (Player player, string opponentConnectionId) RemovePlayer(string connectionId)
    {
        string opponentConnectionId = null;

        if (!_connectionIdToPlayer.TryRemove(connectionId, out var disconnectedPlayer))
        {
            throw new KeyNotFoundException($"Player with connection id '{connectionId}' not found");
        }

        var gameLobby = _gameLobbies.FirstOrDefault(x => x.IsPlayerInLobby(connectionId));
        if (gameLobby is not null)
        {
            opponentConnectionId = gameLobby.GetOpponentConnectionId(connectionId);
            _gameLobbies.Remove(gameLobby);
        }

        return (disconnectedPlayer, opponentConnectionId);
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

    public string MapOpponents(string firstPlayerConnectionId, string secondPlayerConnectionId, GridSize gridSize)
    {
        var gameLobby = new GameLobby(firstPlayerConnectionId, secondPlayerConnectionId, gridSize);
        _gameLobbies.Add(gameLobby);

        return gameLobby.Id;
    }

    public (string opponentConnectionId, int gainPoints, bool isGameEnd) OpponentMakeMove(string lobbyId,
                                                                                          string playerConnectionId,
                                                                                          int x1,
                                                                                          int y1,
                                                                                          int y2)
    {
        var gameLobby = _gameLobbies.FirstOrDefault(x => x.Id == lobbyId);
        if (gameLobby is null)
        {
            throw new InvalidDataException();
        }

        var moveTuple = gameLobby.MakeMove(x1, y1, y2);
        if (moveTuple.isGameEnd)
        {
            _gameLobbies.Remove(gameLobby);
        }

        var opponentConnectionId = gameLobby.GetOpponentConnectionId(playerConnectionId);
        return (opponentConnectionId, moveTuple.gainPoints, moveTuple.isGameEnd);
    }

    public string RemoveLobby(string playerInLobbyConnectionId)
    {
        var gameLobby = _gameLobbies.FirstOrDefault(x => x.IsPlayerInLobby(playerInLobbyConnectionId));
        if (gameLobby is null)
        {
            throw new InvalidDataException();
        }

        _gameLobbies.Remove(gameLobby);
        return gameLobby.GetOpponentConnectionId(playerInLobbyConnectionId);
    }
}