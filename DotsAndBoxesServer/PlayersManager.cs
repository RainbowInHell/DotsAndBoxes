using System.Collections.Concurrent;
using DotsAndBoxesServerAPI;

namespace DotsAndBoxesServer;

public class PlayersManager
{
    private readonly ConcurrentDictionary<string, Player> _connectionIdToPlayer = new();

    private readonly ConcurrentDictionary<string, string> _playerToOpponent = new();

    private readonly List<GameLobby> _gameLobbies = new();

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

    public string MapOpponents(string firstPlayerConnectionId, string secondPlayerConnectionId, GridSize gridSize)
    {
        var gameLobby = new GameLobby(firstPlayerConnectionId, secondPlayerConnectionId, gridSize);
        _gameLobbies.Add(gameLobby);

        return gameLobby.Id;
    }

    public (string opponentConnectionId, int gainPoints, bool isGameEnd) OpponentMakeMove(string lobbyId,
                                                                                          string playerConnectionId,
                                                                                          int startPointX,
                                                                                          int startPointY,
                                                                                          int endPointX,
                                                                                          int endPointY)
    {
        var gameLobby = _gameLobbies.FirstOrDefault(x => x.Id == lobbyId);
        if (gameLobby is null)
        {
            throw new InvalidDataException();
        }

        var points = gameLobby.MakeMove(startPointX, startPointY, endPointX, endPointY);
        var opponentConnectionId = gameLobby.GetOpponentConnectionId(playerConnectionId);
        var isGameEnd = gameLobby.IsGameEnded();

        return (opponentConnectionId, points, isGameEnd);
    }

    public string GetOpponentConnectionId(string connectionId)
    {
        return _playerToOpponent.GetValueOrDefault(connectionId);
    }

    public void GameEnd(string lobbyId)
    {
        var gameLobby = _gameLobbies.FirstOrDefault(x => x.Id == lobbyId);
        if (gameLobby is null)
        {
            throw new InvalidDataException();
        }

        _gameLobbies.Remove(gameLobby);
    }
}