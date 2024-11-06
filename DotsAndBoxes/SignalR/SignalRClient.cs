using System.Configuration;
using AsyncAwaitBestPractices;
using DotsAndBoxesServerAPI;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace DotsAndBoxes;

public sealed class SignalRClient : IAsyncDisposable
{
    public const int MaxReconnectAttempts = 5;

    private readonly IReadOnlyCollection<IDisposable> _eventSubscriptions;

    private readonly CustomRetryPolicy _customRetryPolicy = new();

    private readonly HubConnection _hubConnection;

    private readonly ILogger<SignalRClient> _logger;

    #region PlayerEvents

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

    #region GameEvents

    public event Action<int, int, int, int, int> OnOpponentMakeMove;
    public event Action<int> OnGainPoints;
    public event Func<Task> OnGameEnd;
    public event Func<Task> OnOpponentLeaveGame;

    #endregion

    #region ConnectionEvents

    public event Action OnConnectionLost;
    public event Action OnConnectionRestored;
    public event Action<long> OnReconnectAttempt;

    #endregion

    public SignalRClient(ILogger<SignalRClient> logger)
    {
        _logger = logger;

        var hubAddress = ConfigurationManager.AppSettings["HubAddress"];

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubAddress!)
            .WithAutomaticReconnect(_customRetryPolicy)
            .Build();

        _hubConnection.Reconnecting += OnReconnecting;
        _hubConnection.Reconnected += OnReconnected;
        _hubConnection.Closed += OnConnectionClosed;

        _customRetryPolicy.OnRetry += OnRetryPolicyAttempt;
        _eventSubscriptions = ConstructEventSubscriptions();
    }

    #region Methods

    #region ListenersSetup

    private IReadOnlyCollection<IDisposable> ConstructEventSubscriptions()
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
                                      lobbyId => OnChallengeAccept?.Invoke(lobbyId)),

            _hubConnection.On<int, int, int, int, int>(HubEventActions.GetHubEventActionName(HubEventActionType.OnOpponentMakeMove),
                                      (x1, y1, x2, y2, gainPoints) => OnOpponentMakeMove?.Invoke(x1, y1, x2, y2, gainPoints)),
            
            _hubConnection.On<int>(HubEventActions.GetHubEventActionName(HubEventActionType.OnGainPoints),
                                   gainPoints => OnGainPoints?.Invoke(gainPoints)),

            _hubConnection.On(HubEventActions.GetHubEventActionName(HubEventActionType.OnGameEnd), () =>
                                                                                                       {
                                                                                                           OnGameEnd?.Invoke();
                                                                                                       }),

            _hubConnection.On(HubEventActions.GetHubEventActionName(HubEventActionType.OnOpponentLeaveGame), () =>
                                                                                                              {
                                                                                                                  OnOpponentLeaveGame?.Invoke();
                                                                                                              })
        ];
    }

    #endregion

    #region SenderMethods

    public async Task SendNewPlayerConnectAsync(Player player)
    {
        await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerConnect), player);
    }

    public async Task SendPlayerUpdateSettingsAsync(SettingsHolder newSettings)
    {
        await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerUpdateSettings), newSettings);
    }

    public async Task SendChallengeAsync(string challengedPlayerName)
    {
        await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerSendChallenge), challengedPlayerName);
    }

    public async Task SendChallengeAnswerAsync(bool challengeAccepted, string challengeSenderName)
    {
        await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerSendChallengeAnswer), challengeAccepted, challengeSenderName);
    }

    public async Task UndoChallengeAsync(string challengedPlayerName)
    {
        await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.PlayerCancelChallenge), challengedPlayerName);
    }

    public async Task MakeMoveAsync(string lobbyId, int x1, int y1, int x2, int y2)
    {
        await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.OpponentMakeMove), lobbyId, x1, y1, x2, y2);
    }

    public async Task LeaveGameAsync()
    {
        await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.OpponentLeaveGame));
    }

    #endregion

    #region ConnectionMethods

    public async Task StartConnectionAsync()
    {
        try
        {
            await _hubConnection.StartAsync();
            _logger.LogWithCallerInfo(LogLevel.Information, "Hub connection started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogWithCallerInfo(LogLevel.Error, "Can't start connection due to an error: ", ex);
            throw;
        }
    }

    public async Task StopConnectionAsync()
    {
        try
        {
            await _hubConnection.StopAsync();
            _logger.LogWithCallerInfo(LogLevel.Information, "Hub connection stopped successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogWithCallerInfo(LogLevel.Error, "Can't stop connection due to an error: ", ex);
        }
    }

    private Task OnReconnecting(Exception exception)
    {
        _logger.LogWithCallerInfo(LogLevel.Error, "OnReconnecting exception: ", exception);
        OnConnectionLost?.Invoke();

        return Task.CompletedTask;
    }

    private Task OnReconnected(string connectionId)
    {
        _logger.LogWithCallerInfo(LogLevel.Warning, "Gracefully reconnected to the server.");
        OnConnectionRestored?.Invoke();

        return Task.CompletedTask;
    }

    private Task OnConnectionClosed(Exception exception)
    {
        if (exception is not null)
        {
            _logger.LogWithCallerInfo(LogLevel.Error, "Connection closed due to an error: ", exception);
        }
        else
        {
            _logger.LogWithCallerInfo(LogLevel.Warning, "Connection closed gracefully.");
        }

        return Task.CompletedTask;
    }

    private void OnRetryPolicyAttempt(long attemptNumber)
    {
        OnReconnectAttempt?.Invoke(attemptNumber);

        if (attemptNumber != MaxReconnectAttempts)
        {
            return;
        }

        _logger.LogWithCallerInfo(LogLevel.Warning, $"Failed to reconnect after {MaxReconnectAttempts} attempts.");
        StopConnectionAsync().SafeFireAndForget();
    }

    #endregion

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

    #endregion
}