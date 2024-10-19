using System.Configuration;
using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesServerAPI.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace DotsAndBoxes.SignalR;

public sealed class SignalRServer : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;

    private readonly ILogger<SignalRServer> _logger;

    #region StateEvents

    public Action<Player> OnPlayerConnectAction { get; set; }
    public Action<string> OnPlayerDisconnectAction { get; set; }
    public Action<Player> OnPlayerUpdateSettingsAction { get; set; }
    public Action<string, PlayerStatus> OnPlayerChangeStatusAction { get; set; }

    public Action OnReconnectingAction { get; set; }
    public Action OnReconnectedAction { get; set; }

    #endregion

    #region ChallengeEvents

    public Action<string> OnChallengeAction { get; set; }
    public Action OnChallengeCancelAction { get; set; }
    public Action OnChallengeRejectAction { get; set; }
    public Action<string> OnChallengeAcceptAction { get; set; }

    #endregion

    public SignalRServer(ILogger<SignalRServer> logger)
    {
        _logger = logger;

        var hubAddress = ConfigurationManager.AppSettings["HubAddress"];

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubAddress!)
            .WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30)])
            .Build();

        _hubConnection.Closed += OnConnectionClosed;
        _hubConnection.Reconnecting += OnReconnecting;
        _hubConnection.Reconnected += OnReconnected;

        SetupServerEventsListening();
    }

    #region Methods
    
    public async Task StartConnectionAsync()
    {
        try
        {
            await _hubConnection.StartAsync();
            _logger.LogInformation("Hub connection started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't start connection due to an error: {ex}", ex);
            throw;
        }
    }

    #region ListenersSetup

    private void SetupServerEventsListening()
    {
        _hubConnection.On<Player>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerConnect),
                                  newPlayer => OnPlayerConnectAction?.Invoke(newPlayer));

        _hubConnection.On<string>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerDisconnect),
                                  disconnectedPlayerName => OnPlayerDisconnectAction?.Invoke(disconnectedPlayerName));

        _hubConnection.On<Player>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerUpdateSettings),
                                  updatedPlayer => OnPlayerUpdateSettingsAction?.Invoke(updatedPlayer));

        _hubConnection.On<string>(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallenge),
                                  challengeSenderName => OnChallengeAction?.Invoke(challengeSenderName));

        _hubConnection.On<string, PlayerStatus>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus),
                                  (playerName, newStatus) => OnPlayerChangeStatusAction?.Invoke(playerName, newStatus));

        _hubConnection.On(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeCancel), () =>
                                                                                                           {
                                                                                                               OnChallengeCancelAction?.Invoke();
                                                                                                           });

        _hubConnection.On(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeReject), () =>
                                                                                                           {
                                                                                                               OnChallengeRejectAction?.Invoke();
                                                                                                           });

        _hubConnection.On<string>(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeAccept),
                                  challengedPlayerName => OnChallengeAcceptAction?.Invoke(challengedPlayerName));
    }

    #endregion

    #region Senders

    public async Task SendNewPlayerConnectAsync(Player player)
    {
        try
        {
            await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerConnect), player);
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't send notification about new player due to an error: {ex}", ex);
            throw;
        }
    }

    public async Task SendPlayerUpdateSettingsAsync(SettingsHolder newSettings)
    {
        try
        {
            await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerUpdateSettings), newSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't send updated player's settings due to an error: {ex}", ex);
            throw;
        }
    }

    public async Task SendChallengeAsync(string challengedPlayerName)
    {
        try
        {
            await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerSendChallenge), challengedPlayerName);
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't send challenge due to an error: {ex}", ex);
            throw;
        }
    }

    public async Task SendChallengeAnswerAsync(bool challengeAccepted, string challengeSenderName)
    {
        try
        {
            await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerSendChallengeAnswer), challengeAccepted, challengeSenderName);
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't send challenge answer due to an error: {ex}", ex);
            throw;
        }
    }

    public async Task UndoChallengeAsync(string challengedPlayerName)
    {
        try
        {
            await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerCancelChallenge), challengedPlayerName);
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't undo challenge due to an error: {ex}", ex);
            throw;
        }
    }

    #endregion

    private Task OnReconnecting(Exception exception)
    {
        _logger.LogWarning("Attempting to reconnect...");
        OnReconnectingAction?.Invoke();
        return Task.CompletedTask;
    }

    private Task OnReconnected(string connectionId)
    {
        _logger.LogInformation("Reconnected successfully. Connection ID: {connectionId}", connectionId);
        OnReconnectedAction?.Invoke();
        return Task.CompletedTask;
    }

    private Task OnConnectionClosed(Exception exception)
    {
        if (exception is not null)
        {
            _logger.LogError("Connection closed due to an error: {ex}", exception);
        }
        else
        {
            _logger.LogWarning("Connection closed gracefully.");
        }

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            _hubConnection.Closed -= OnConnectionClosed;
            await _hubConnection.StopAsync();
        }
    }

    #endregion
}