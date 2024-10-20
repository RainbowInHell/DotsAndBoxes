using System.Configuration;
using AsyncAwaitBestPractices;
using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesServerAPI.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace DotsAndBoxes.SignalR;

public sealed class SignalRClient : IAsyncDisposable
{
    public const int MaxReconnectAttempts = 5;

    private HubConnectionState _connectionState = HubConnectionState.Disconnected;

    private readonly IReadOnlyCollection<IDisposable> _eventSubscriptions;

    private readonly CustomRetryPolicy _customRetryPolicy = new();

    private readonly HubConnection _hubConnection;

    private readonly ILogger<SignalRClient> _logger;

    #region StateEvents

    public event Action<Player> OnPlayerConnect;

    public event Action<string> OnPlayerDisconnect;
    public event Action<Player> OnPlayerUpdateSettings;

    public event Action<string, PlayerStatus> OnPlayerChangeStatus;

    #endregion

    #region ChallengeEvents

    public event Action<string> OnChallenge;

    public event Action OnChallengeCancel;

    public event Action OnChallengeReject;

    public event Action<string> OnChallengeAccept;

    #endregion

    public event Action<HubConnectionState> OnConnectionStateChanged;

    public event Action<long> ReconnectAttempt;

    public SignalRClient(ILogger<SignalRClient> logger)
    {
        _logger = logger;

        var hubAddress = ConfigurationManager.AppSettings["HubAddress"];

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubAddress!)
            .WithAutomaticReconnect(_customRetryPolicy)
            .Build();

        _hubConnection.Closed += OnConnectionClosed;
        _hubConnection.Reconnecting += OnReconnecting;
        _hubConnection.Reconnected += OnReconnected;

        _customRetryPolicy.OnRetry += OnRetryPolicyAttempt;
        _eventSubscriptions = ConstructEventSubscriptions();
    }

    #region Methods
    
    public async Task StartConnectionAsync()
    {
        try
        {
            await _hubConnection.StartAsync();
            SetConnectionStateTo(HubConnectionState.Connected);
            // _logger.LogInformation("Hub connection started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't start connection due to an error: {ex}", ex);
            throw;
        }
    }

    public async Task StopConnectionAsync()
    {
        try
        {
            await _hubConnection.StopAsync();
            SetConnectionStateTo(HubConnectionState.Connected);
            // _logger.LogInformation("Hub connection stopped successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't start connection due to an error: {ex}", ex);
            throw;
        }
    }

    #region ListenersSetup

    private List<IDisposable> ConstructEventSubscriptions()
    {
        return
        [
            _hubConnection.On<Player>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerConnect),
                                      newPlayer => OnPlayerConnect?.Invoke(newPlayer)),

            _hubConnection.On<string>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerDisconnect),
                                      disconnectedPlayerName => OnPlayerDisconnect?.Invoke(disconnectedPlayerName)),

            _hubConnection.On<Player>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerUpdateSettings),
                                      updatedPlayer => OnPlayerUpdateSettings?.Invoke(updatedPlayer)),

            _hubConnection.On<string>(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallenge),
                                      challengeSenderName => OnChallenge?.Invoke(challengeSenderName)),

            _hubConnection.On<string, PlayerStatus>(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus),
                                                    (playerName, newStatus) => OnPlayerChangeStatus?.Invoke(playerName, newStatus)),

            _hubConnection.On(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeCancel), () =>
                                                                                                               {
                                                                                                                   OnChallengeCancel?.Invoke();
                                                                                                               }),

            _hubConnection.On(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeReject), () =>
                                                                                                               {
                                                                                                                   OnChallengeReject?.Invoke();
                                                                                                               }),

            _hubConnection.On<string>(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeAccept),
                                      challengedPlayerName => OnChallengeAccept?.Invoke(challengedPlayerName))
        ];
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
        SetConnectionStateTo(HubConnectionState.Reconnecting);
        OnConnectionStateChanged?.Invoke(_connectionState);

        return Task.CompletedTask;
    }

    private void OnRetryPolicyAttempt(long attemptNumber)
    {
        ReconnectAttempt?.Invoke(attemptNumber);

        if (attemptNumber == MaxReconnectAttempts)
        {
            _hubConnection.StopAsync().SafeFireAndForget();
        }
    }

    private Task OnReconnected(string connectionId)
    {
        SetConnectionStateTo(HubConnectionState.Connected);
        OnConnectionStateChanged?.Invoke(_connectionState);

        return Task.CompletedTask;
    }

    private Task OnConnectionClosed(Exception exception)
    {
        SetConnectionStateTo(HubConnectionState.Disconnected);

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
        foreach (var subscription in _eventSubscriptions)
        {
            subscription.Dispose();
        }

        _customRetryPolicy.OnRetry -= OnRetryPolicyAttempt;
        _hubConnection.Closed -= OnConnectionClosed;
        await _hubConnection.DisposeAsync();
    }

    private void SetConnectionStateTo(HubConnectionState newConnectionState)
    {
        if (_connectionState == newConnectionState)
        {
            return;
        }

        _connectionState = newConnectionState;
        OnConnectionStateChanged?.Invoke(_connectionState);
    }

    #endregion
}