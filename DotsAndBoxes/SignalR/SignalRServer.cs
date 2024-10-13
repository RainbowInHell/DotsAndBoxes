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

    #region ServerEventHandlers

    public Action<Player> OnNewPlayerConnectAction { get; set; }

    public Action<string> OnPlayerDisconnectAction { get; set; }

    public Action<Player> OnPlayerUpdateSettingsAction { get; set; }

    public Action<string> OnPlayerSendChallengeAction { get; set; }

    public Action<string> OnPlayerReceiveChallengeAction { get; set; }

    public Action<string> OnPlayerUndoChallengeAction { get; set; }

    public Action<string> OnPlayerRejectChallengeAction { get; set; }

    #endregion

    public SignalRServer(ILogger<SignalRServer> logger)
    {
        _logger = logger;

        var hubAddress = ConfigurationManager.AppSettings["HubAddress"];

        _hubConnection = new HubConnectionBuilder().WithUrl(hubAddress!).Build();
        _hubConnection.Closed += OnConnectionClosed;

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
        _hubConnection.On<Player>(HubEventActions.GetHubEventActionName(HubEventActionType.OnNewPlayerConnect),
                                  newPlayer => OnNewPlayerConnectAction?.Invoke(newPlayer));

        _hubConnection.On<string>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerDisconnect),
                                  disconnectedPlayerName => OnPlayerDisconnectAction?.Invoke(disconnectedPlayerName));

        _hubConnection.On<Player>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerUpdateSettings),
                                  updatedPlayer => OnPlayerUpdateSettingsAction?.Invoke(updatedPlayer));

        _hubConnection.On<string>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerSendChallenge),
                                  challengeSenderName => OnPlayerSendChallengeAction?.Invoke(challengeSenderName));

        _hubConnection.On<string>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerReceiveChallenge),
                                  challengeReceiverName => OnPlayerReceiveChallengeAction?.Invoke(challengeReceiverName));

        _hubConnection.On<string>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerCancelChallenge),
                                  challengeCancelerName => OnPlayerUndoChallengeAction?.Invoke(challengeCancelerName));

        _hubConnection.On<string>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerRejectChallenge),
                                  challengeRejectorName => OnPlayerRejectChallengeAction?.Invoke(challengeRejectorName));
    }

    #endregion

    #region Senders

    public async Task SendNewPlayerConnectAsync(Player player)
    {
        try
        {
            await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.NewPlayerConnect), player);
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

    public async Task SendChallengeAsync(string toPlayerName)
    {
        try
        {
            await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerSendChallenge), toPlayerName);
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

    public async Task UndoChallengeAsync(string toPlayerName)
    {
        try
        {
            await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerCancelChallenge), toPlayerName);
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't undo challenge due to an error: {ex}", ex);
            throw;
        }
    }

    #endregion

    private Task OnConnectionClosed(Exception exception)
    {
        if (exception is not null)
        {
            _logger.LogError("Connection closed due to an error: {ex}", exception);
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