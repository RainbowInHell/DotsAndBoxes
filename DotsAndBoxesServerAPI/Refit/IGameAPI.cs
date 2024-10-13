using DotsAndBoxesServerAPI.Models;
using Refit;

namespace DotsAndBoxesServerAPI.Refit;

public interface IGameAPI
{
    [Get("/players")]
    Task<IEnumerable<Player>> GetConnectedPlayers();
}