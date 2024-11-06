using DotsAndBoxesServerAPI;

namespace DotsAndBoxesServer;

public class GameLobby
{
    private readonly string _firstPlayerConnectionId;

    private readonly string _secondPlayerConnectionId;

    private readonly GameController _gameController;

    public string Id { get; }

    public GameLobby(string firstPlayerConnectionId, string secondPlayerConnectionId, GridSize gridSize)
    {
        _firstPlayerConnectionId = firstPlayerConnectionId;
        _secondPlayerConnectionId = secondPlayerConnectionId;
        _gameController = new GameController(gridSize);

        Id = Guid.NewGuid().ToString();
    }

    public (int gainPoints, bool isGameEnd) MakeMove(int x1, int y1, int y2)
    { 
        return (_gameController.MakeMove(x1, y1, y2), _gameController.IsGameEnded());
    }

    public string GetOpponentConnectionId(string playerConnectionId)
    {
        return _firstPlayerConnectionId == playerConnectionId
                   ? _secondPlayerConnectionId
                   : _firstPlayerConnectionId;
    }

    public bool IsPlayerInLobby(string playerConnectionId)
    {
        return _firstPlayerConnectionId == playerConnectionId
               || _secondPlayerConnectionId == playerConnectionId;
    }
}