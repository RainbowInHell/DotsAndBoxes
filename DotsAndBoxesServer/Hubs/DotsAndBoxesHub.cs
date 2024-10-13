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

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var disconnectedPlayer = _playersManager.RemovePlayer(Context.ConnectionId);
 
        await Clients.All.SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerDisconnect),
                                    disconnectedPlayer.Name);
    }

    [HubMethodName(nameof(ServerMethodType.NewPlayerConnect))]
    public async Task NewPlayerConnectedAsync(Player player)
    {
        _playersManager.AddPlayer(Context.ConnectionId, player);

        await Clients.AllExcept(Context.ConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnNewPlayerConnect), player);
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
        // Create a new group in which the player who sent the challenge is the host of the group.
        var newGroup = _playersManager.CreateGroup(Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, newGroup.Name);

        // Get the connection id of the challenged player.
        var challengeReceiverConnectionId = _playersManager.GetPlayerConnectionId(toPlayerName);
        var challengeReceiverName = _playersManager.GetConnectedPlayer(challengeReceiverConnectionId).Name;

        // Get the name of the player who sent the challenge.
        var challengeSenderName = _playersManager.GetConnectedPlayer(Context.ConnectionId).Name;

        // Notify all except challenge sender and challenge receiver with the name of the challenged player
        // in order to make him unselectable for challenging.
        await Clients.AllExcept(Context.ConnectionId, challengeReceiverConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerReceiveChallenge), challengeReceiverName);

        // Notify the challenged player with the name of the player who challenged him.
        await Clients.Client(challengeReceiverConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerSendChallenge), challengeSenderName);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerCancelChallenge))]
    public async Task PlayerUndoChallengeAsync(string toPlayerName)
    {
        // Delete managed group, because no need to store object in the memory.
        var deletedGroup = _playersManager.DeleteGroup(Context.ConnectionId);

        // Delete server group.
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, deletedGroup.Name);

        // Get the connection id of the challenged player.
        var challengeReceiverConnectionId = _playersManager.GetPlayerConnectionId(toPlayerName);

        // Get the name of the player who sent the request.
        var challengeSenderName = _playersManager.GetConnectedPlayer(Context.ConnectionId);

        await Clients.Client(challengeReceiverConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerCancelChallenge), challengeSenderName.Name);

        await Clients.AllExcept(Context.ConnectionId, challengeReceiverConnectionId)
                     .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerCancelChallenge), toPlayerName);
    }

    [HubMethodName(nameof(ServerMethodType.PlayerSendChallengeAnswer))]
    public async Task PlayerSendChallengeAnswerAsync(bool challengeAccepted, string challengeSenderName)
    {
        var groupHostConnectionId = _playersManager.GetPlayerConnectionId(challengeSenderName);

        // Challenge request was accepted.
        if (challengeAccepted)
        {
            // We can identify group by host player connectionId,
            // Thus, pass it and connection id of the player who accept the request.
            _playersManager.AddToGroup(groupHostConnectionId, Context.ConnectionId);
        }
        // Challenge request was rejected.
        else
        {
            // Delete managed group, because no need to store object in the memory.
            var deletedGroup = _playersManager.DeleteGroup(groupHostConnectionId);

            // Delete server group.
            await Groups.RemoveFromGroupAsync(groupHostConnectionId, deletedGroup.Name);

            // Notify all ...
            var playerWhoRejectChallenge = _playersManager.GetConnectedPlayer(Context.ConnectionId).Name;
            await Clients.AllExcept(Context.ConnectionId)
                         .SendAsync(HubEventActions.GetHubEventActionName(HubEventActionType.OnPlayerRejectChallenge), playerWhoRejectChallenge);
        }
    }
}