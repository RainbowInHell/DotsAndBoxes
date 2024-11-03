using DotsAndBoxesServerAPI;
using DotsAndBoxesServerAPI.Models;

namespace DotsAndBoxesServer;

public class GameLobby
{
    private readonly string _firstPlayerConnectionId;

    private readonly string _secondPlayerConnectionId;

    // private readonly GridSize _gridSize;

    private readonly GameController _gameController;

    public string Id { get; }

    public GameLobby(string firstPlayerConnectionId, string secondPlayerConnectionId, GridSize gridSize)
    {
        _firstPlayerConnectionId = firstPlayerConnectionId;
        _secondPlayerConnectionId = secondPlayerConnectionId;
        _gameController = new GameController(gridSize);

        Id = Guid.NewGuid().ToString();
    }

    public int MakeMove(int startPointX, int startPointY, int endPointX, int endPointY)
    { 
        return _gameController.MakeMove(startPointX, startPointY, endPointX, endPointY);
    }

    public bool IsGameEnded()
    {
        return _gameController.IsGameEnded();
    }

    public string GetOpponentConnectionId(string playerConnectionId)
    {
        return _firstPlayerConnectionId == playerConnectionId
                   ? _secondPlayerConnectionId
                   : _firstPlayerConnectionId;
    }
}