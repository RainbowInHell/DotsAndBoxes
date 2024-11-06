using DotsAndBoxesServerAPI;
using Microsoft.AspNetCore.SignalR;

namespace DotsAndBoxesServer;

public class DotsAndBoxesHub : Hub
{
    private readonly GameManager _gameManager;

    public DotsAndBoxesHub(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var (disconnectedPlayer, opponentConnectionId) = _gameManager.RemovePlayer(Context.ConnectionId);
        if (opponentConnectionId != null)
        {
            await Clients.Client(opponentConnectionId)
                .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnOpponentLeaveGame));
        }

        await Clients.All.SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerDisconnect),
                                    disconnectedPlayer.Name);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerConnect))]
    public async Task NewPlayerConnectedAsync(Player player)
    {
        _gameManager.AddPlayer(Context.ConnectionId, player);

        await Clients.AllExcept(Context.ConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerConnect), player);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerUpdateSettings))]
    public async Task PlayerUpdateSettingsAsync(SettingsHolder newSettings)
    {
        var updatedPlayer = _gameManager.UpdatePlayer(Context.ConnectionId, newSettings);

        await Clients.AllExcept(Context.ConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerUpdateSettings), updatedPlayer);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerSendChallenge))]
    public async Task PlayerSendChallengeAsync(string toPlayerName)
    {
        var challengeReceiverConnectionId = _gameManager.GetConnectionId(toPlayerName);
        var challengeReceiverName = _gameManager.GetConnectedPlayer(challengeReceiverConnectionId).Name;

        var challengeSenderName = _gameManager.GetConnectedPlayer(Context.ConnectionId).Name;

        await Clients.Client(challengeReceiverConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallenge), challengeSenderName);

        await Clients.AllExcept(Context.ConnectionId, challengeReceiverConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengeReceiverName, PlayerStatus.Challenged);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerCancelChallenge))]
    public async Task PlayerCancelChallengeAsync(string challengedPlayerName)
    {
        var challengedPlayerConnectionId = _gameManager.GetConnectionId(challengedPlayerName);

        await Clients.Client(challengedPlayerConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeCancel));

        await Clients.AllExcept(Context.ConnectionId, challengedPlayerConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengedPlayerName, PlayerStatus.FreeToPlay);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerSendChallengeAnswer))]
    public async Task PlayerSendChallengeAnswerAsync(bool challengeAccepted, string challengeSenderName)
    {
        var challengeSenderConnectionId = _gameManager.GetConnectionId(challengeSenderName);
        var challengedPlayerConnectionId = Context.ConnectionId;

        var challengedPlayer = _gameManager.GetConnectedPlayer(Context.ConnectionId);

        // Challenge was accepted.
        if (challengeAccepted)
        {
            var gameLobbyId = _gameManager.MapOpponents(challengeSenderConnectionId, challengedPlayerConnectionId, challengedPlayer.Settings.GridSize);

            await Clients.Clients(challengeSenderConnectionId, challengedPlayerConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeAccept), gameLobbyId);

            await Clients.AllExcept(challengeSenderConnectionId, challengedPlayerConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengedPlayer.Name, PlayerStatus.Playing);
            await Clients.AllExcept(challengeSenderConnectionId, challengedPlayerConnectionId)
                          .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengeSenderName, PlayerStatus.Playing);
        }
        // Challenge was rejected.
        else
        {
            await Clients.Client(challengeSenderConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeReject));

            await Clients.AllExcept(challengeSenderConnectionId, challengedPlayerConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengedPlayer.Name, PlayerStatus.FreeToPlay);
        }
    }

    [HubMethodName(nameof(ServerMethodType.OpponentMakeMove))]
    public async Task OpponentMakeMoveAsync(string lobbyId, int x1, int y1, int x2, int y2)
    {
        var gameStateTuple = _gameManager.OpponentMakeMove(lobbyId, Context.ConnectionId, x1, y1, y2);

        await Clients.Client(Context.ConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnGainPoints), gameStateTuple.gainPoints);

        await Clients.Client(gameStateTuple.opponentConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnOpponentMakeMove), x1, y1, x2, y2, gameStateTuple.gainPoints);

        if (gameStateTuple.isGameEnd)
        {
            await Clients.Clients(Context.ConnectionId, gameStateTuple.opponentConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnGameEnd));
        }
    }

    [HubMethodName(nameof(ServerMethodType.OpponentLeaveGame))]
    public async Task OnOpponentLeaveGameAsync()
    {
        var leavedPlayer = _gameManager.GetConnectedPlayer(Context.ConnectionId);
        var opponentConnectionId = _gameManager.RemoveLobby(Context.ConnectionId);
        var opponent = _gameManager.GetConnectedPlayer(opponentConnectionId);

        await Clients.Client(opponentConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnOpponentLeaveGame));
        await Clients.AllExcept(Context.ConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), leavedPlayer.Name, leavedPlayer.Status);
        await Clients.AllExcept(opponentConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), opponent.Name, opponent.Status);
    }
}