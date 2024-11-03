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

    public async Task MakeMoveAsync(string lobbyId, int x1, int y1, int x2, int y2)
    {
        try
        {
            await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.OpponentMakeMove), lobbyId, x1, y1, x2, y2);
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't make move due to an error: {ex}", ex);
            throw;
        }
    }

    public async Task LeaveGameAsync()
    {
        try
        {
            await _hubConnection.SendAsync(ServerMethods.GetServerMethodName(ServerMethodType.OpponentLeaveGame));
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't leave game due to an error: {ex}", ex);
            throw;
        }
    }

    #endregion

    #region ConnectionMethods

    public async Task StartConnectionAsync()
    {
        try
        {
            await _hubConnection.StartAsync();
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
            // _logger.LogInformation("Hub connection started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Can't stop connection due to an error: {ex}", ex);
            throw;
        }
    }

    private async Task OnReconnecting(Exception exception)
    {
        _logger.LogError("OnReconnecting exception: {exception}", exception);
        // await _hubConnection.StopAsync().ConfigureAwait(false);

        OnConnectionLost?.Invoke();
        // return Task.CompletedTask;
    }

    private void OnRetryPolicyAttempt(long attemptNumber)
    {
        OnReconnectAttempt?.Invoke(attemptNumber);

        if (attemptNumber == MaxReconnectAttempts)
        {
            _hubConnection.StopAsync().SafeFireAndForget(onException: ex => _logger.LogError("Failed to reconnect after {attempts} attempts due to an error: {ex}.", MaxReconnectAttempts, ex));
        }
    }

    private Task OnReconnected(string connectionId)
    {
        OnConnectionRestored?.Invoke();
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