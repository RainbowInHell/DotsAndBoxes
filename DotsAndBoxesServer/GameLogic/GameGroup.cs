using DotsAndBoxesServerAPI.Models;

namespace DotsAndBoxesServer.GameLogic;

public sealed class GameGroup
{
    private Player _firstPlayer;

    private Player _secondPlayer;

    public string Name { get; } = Guid.NewGuid().ToString();

    public void AddPlayer(Player player)
    {
        if (_firstPlayer is null)
        {
            _firstPlayer = player;
        }
        else
        {
            _secondPlayer = player;
        }
    }
}