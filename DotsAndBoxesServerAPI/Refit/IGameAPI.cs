using DotsAndBoxesServerAPI.Models;
using Refit;

namespace DotsAndBoxesServerAPI.Refit;

public interface IGameAPI
{

    [Get("/players/{name}")]
    Task<Player> GetConnectedPlayerByNameAsync(string name, CancellationToken cancellationToken = default);
}