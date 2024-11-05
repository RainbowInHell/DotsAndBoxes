using Refit;

namespace DotsAndBoxesServerAPI;

public interface IGameAPI
{
    [Get("/players")]
    Task<IEnumerable<Player>> GetConnectedPlayers();
}