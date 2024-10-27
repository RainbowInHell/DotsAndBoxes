using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesServerAPI.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace DotsAndBoxesServer.Hubs;

public class DotsAndBoxesHub : Hub
{
    private readonly PlayersManager _playersManager;

    public DotsAndBoxesHub(PlayersManager playersManager)
    {
        _playersManager = playersManager;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var disconnectedPlayer = _playersManager.RemovePlayer(Context.ConnectionId);

        var opponentConnectionId = _playersManager.GetOpponentConnectionId(Context.ConnectionId);
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
        _playersManager.AddPlayer(Context.ConnectionId, player);

        await Clients.AllExcept(Context.ConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerConnect), player);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerUpdateSettings))]
    public async Task PlayerUpdateSettingsAsync(SettingsHolder newSettings)
    {
        var updatedPlayer = _playersManager.UpdatePlayer(Context.ConnectionId, newSettings);

        await Clients.AllExcept(Context.ConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerUpdateSettings), updatedPlayer);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerSendChallenge))]
    public async Task PlayerSendChallengeAsync(string toPlayerName)
    {
        var challengeReceiverConnectionId = _playersManager.GetConnectionId(toPlayerName);
        var challengeReceiverName = _playersManager.GetConnectedPlayer(challengeReceiverConnectionId).Name;

        var challengeSenderName = _playersManager.GetConnectedPlayer(Context.ConnectionId).Name;

        await Clients.Client(challengeReceiverConnectionId)
            .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallenge), challengeSenderName);

        await Clients.AllExcept(Context.ConnectionId, challengeReceiverConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengeReceiverName, PlayerStatus.Challenged);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerCancelChallenge))]
    public async Task PlayerCancelChallengeAsync(string challengedPlayerName)
    {
        var challengedPlayerConnectionId = _playersManager.GetConnectionId(challengedPlayerName);

        await Clients.Client(challengedPlayerConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeCancel));

        await Clients.AllExcept(Context.ConnectionId, challengedPlayerConnectionId)
            .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengedPlayerName, PlayerStatus.FreeToPlay);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerSendChallengeAnswer))]
    public async Task PlayerSendChallengeAnswerAsync(bool challengeAccepted, string challengeSenderName)
    {
        var challengeSenderConnectionId = _playersManager.GetConnectionId(challengeSenderName);
        var challengedPlayerConnectionId = Context.ConnectionId;

        var challengedPlayerName = _playersManager.GetConnectedPlayer(Context.ConnectionId).Name;

        // Challenge was accepted.
        if (challengeAccepted)
        {
            _playersManager.MapOpponents(challengeSenderConnectionId, challengedPlayerConnectionId);

            await Clients.Clients(challengeSenderConnectionId, challengedPlayerConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeAccept));

            await Clients.AllExcept(challengeSenderConnectionId, challengedPlayerConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengedPlayerName, PlayerStatus.Playing);
            await Clients.AllExcept(challengeSenderConnectionId, challengedPlayerConnectionId)
                          .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengeSenderName, PlayerStatus.Playing);
        }
        // Challenge was rejected.
        else
        {
            await Clients.Client(challengeSenderConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeReject));

            await Clients.AllExcept(challengeSenderConnectionId, challengedPlayerConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengedPlayerName, PlayerStatus.FreeToPlay);
        }
    }

    [HubMethodName(nameof(ServerMethodType.OpponentMakeMove))]
    public async Task OpponentMakeMoveAsync(int x1, int y1, int x2, int y2)
    {
        var opponentConnectionId = _playersManager.GetOpponentConnectionId(Context.ConnectionId);
        await Clients.Client(opponentConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnOpponentMakeMove), x1, y1, x2, y2);
    }

    [HubMethodName(nameof(ServerMethodType.OpponentLeaveGame))]
    public async Task OnOpponentLeaveGameAsync()
    {
        var leavedPlayer = _playersManager.GetConnectedPlayer(Context.ConnectionId);
        var opponentConnectionId = _playersManager.GetOpponentConnectionId(Context.ConnectionId);

        await Clients.Client(opponentConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnOpponentLeaveGame));
        await Clients.AllExcept(Context.ConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), leavedPlayer.Name, leavedPlayer.Status);
    }

    [HubMethodName(nameof(ServerMethodType.OpponentWinGame))]
    public async Task OpponentWinGameAsync()
    {
        var opponentConnectionId = _playersManager.GetOpponentConnectionId(Context.ConnectionId);

        await Clients.Client(opponentConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnOpponentWinGame));
    }
}