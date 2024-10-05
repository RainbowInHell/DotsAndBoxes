using System.Configuration;
using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesServerAPI.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace DotsAndBoxes.SignalR;

// TODO: Мб перенести в API сборку.
public sealed class SignalRServer : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;

    public string ConnectionId => _hubConnection.ConnectionId;

    public Action<Player> OnNewPlayerConnectedAction { get; set; }

    public Action<List<Player>> OnConnectedPlayersActualizationAction { get; set; }

    public SignalRServer()
    {
        var serverAddress = ConfigurationManager.AppSettings["ServerAddress"];

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(serverAddress!)
            .ConfigureLogging(logging =>
                                  {
                                      logging.SetMinimumLevel(LogLevel.Information);
                                      // logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                                      // logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                                      // logging.SetMinimumLevel(LogLevel.Debug);
                                  })
            .Build();

        SetupServerEventsListening();
    }

    public async Task StartConnectionAsync()
    {
        await _hubConnection.StartAsync().ConfigureAwait(false);
    }

    public async Task SendNewPlayerConnectedAsync(Player player)
    {
        await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.NewPlayerConnected), player).ConfigureAwait(false);
    }

    public async Task SendConnectedPlayersActualizationAsync()
    {
        await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.ConnectedPlayersActualization), ConnectionId).ConfigureAwait(false);
    }

    private void SetupServerEventsListening()
    {
        _hubConnection.On<Player>(HubEventActions.GetHubEventActionName(HubEventActionType.OnNewPlayerConnected),
                                  player => OnNewPlayerConnectedAction?.Invoke(player));

        _hubConnection.On<List<Player>>(HubEventActions.GetHubEventActionName(HubEventActionType.OnConnectedPlayersActualization),
                                        players => OnConnectedPlayersActualizationAction?.Invoke(players));
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
            await _hubConnection.StopAsync().ConfigureAwait(false);
    }
}