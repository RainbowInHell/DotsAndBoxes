using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesServerAPI.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace DotsAndBoxesServer.Hubs;

public class DotsAndBoxesHub : Hub
{
    public override Task OnConnectedAsync()
    // public override async Task OnConnectedAsync()
    {
        // await Clients.AllExcept(Context.ConnectionId).SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnNewPlayerConnected), player);
        return base.OnConnectedAsync();
    }

    [HubMethodName(nameof(ServerMethodType.NewPlayerConnected))]
    public async Task NewPlayerConnectedAsync(Player player)
    {
        await Clients.AllExcept(player.ConnectionId).SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnNewPlayerConnected), player);
    }

    [HubMethodName(nameof(ServerMethodType.ConnectedPlayersActualization))]
    public async Task ConnectedPlayersActualizationAsync(string connectionId)
    {
        await Clients.Client(connectionId).SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnConnectedPlayersActualization), new List<Player> { new() { Name = "John Doe" } });
    }
}