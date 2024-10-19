using DotsAndBoxesServer.GameLogic;
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

    #region StateEvents

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var disconnectedPlayer = _playersManager.RemovePlayer(Context.ConnectionId);
 
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

    #endregion

    [HubMethodName(nameof(ServerMethodType.PlayerSendChallenge))]
    public async Task PlayerSendChallengeAsync(string toPlayerName)
    {
        // Create a new group in which the player who sent the challenge is the host of the group.
        var newGroup = _playersManager.CreateGroup(Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, newGroup.Name);

        // Get the connection id of the challenged player.
        var challengeReceiverConnectionId = _playersManager.GetConnectionId(toPlayerName);
        var challengeReceiverName = _playersManager.GetConnectedPlayer(challengeReceiverConnectionId).Name;

        // Get the name of the player who sent the challenge.
        var challengeSenderName = _playersManager.GetConnectedPlayer(Context.ConnectionId).Name;

        // Notify the challenged player with the name of the player who challenged him.
        await Clients.Client(challengeReceiverConnectionId)
            .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallenge), challengeSenderName);

        await Clients.AllExcept(Context.ConnectionId, challengeReceiverConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengeReceiverName, PlayerStatus.Challenged);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerCancelChallenge))]
    public async Task PlayerCancelChallengeAsync(string challengedPlayerName)
    {
        // Delete managed group, because no need to store object in the memory.
        var deletedGroup = _playersManager.DeleteGroup(Context.ConnectionId);

        // Delete server group.
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, deletedGroup.Name);

        // Get the connection id of the challenged player.
        var challengedPlayerConnectionId = _playersManager.GetConnectionId(challengedPlayerName);

        // Notify challenged player that the challenge sender has canceled his offer.
        await Clients.Client(challengedPlayerConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeCancel));

        // Notify all players that the challenged player now is free.
        await Clients.AllExcept(Context.ConnectionId, challengedPlayerConnectionId)
            .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengedPlayerName, PlayerStatus.FreeToPlay);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerSendChallengeAnswer))]
    public async Task PlayerSendChallengeAnswerAsync(bool challengeAccepted, string challengeSenderName)
    {
        var groupHostConnectionId = _playersManager.GetConnectionId(challengeSenderName);
        var challengedPlayerName = _playersManager.GetConnectedPlayer(Context.ConnectionId).Name;

        // Challenge was accepted.
        if (challengeAccepted)
        {
            _playersManager.AddToGroup(groupHostConnectionId, Context.ConnectionId);

            await Clients.Client(groupHostConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeAccept), challengedPlayerName);

            await Clients.AllExcept(Context.ConnectionId, groupHostConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengedPlayerName, PlayerStatus.Playing);
        }
        // Challenge was rejected.
        else
        {
            var deletedGroup = _playersManager.DeleteGroup(groupHostConnectionId);

            await Groups.RemoveFromGroupAsync(groupHostConnectionId, deletedGroup.Name);

            await Clients.Client(groupHostConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnChallengeReject));

            await Clients.AllExcept(Context.ConnectionId, groupHostConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerChangeStatus), challengedPlayerName, PlayerStatus.FreeToPlay);
        }
    }
}