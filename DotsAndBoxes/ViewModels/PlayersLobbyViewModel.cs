using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using DotsAndBoxes.SelectableItems;
using DotsAndBoxes.SignalR;
using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesServerAPI.Refit;
using DotsAndBoxesUIComponents;

namespace DotsAndBoxes.ViewModels;

[Route(Routes.PlayersLobby)]
public sealed partial class PlayersLobbyViewModel : BaseViewModel
{
    #region Fields

    private GridToPlayType _selectedGridType = GridToPlayType.Default;

    private SettingsHolder _currentSettings;

    #region FieldsWithObservableProperties

    [ObservableProperty]
    private GridToPlaySize _selectedGridSize = GridToPlaySize.ThreeToThree;

    [ObservableProperty]
    private bool _doNotDisturb;

    [ObservableProperty]
    private bool _settingsUpdateStarted;

    [ObservableProperty]
    private bool _receiveChallenge;

    [ObservableProperty]
    private string _challengeSenderName;

    [ObservableProperty]
    private string _challengeMessage;

    #endregion

    private readonly INavigationService<BaseViewModel> _navigationService;

    private readonly IGameAPI _gameAPI;

    private readonly SignalRServer _signalRServer;

    #endregion

    public PlayersLobbyViewModel(INavigationService<BaseViewModel> navigationService, IGameAPI gameAPI, SignalRServer signalRServer)
    {
        ViewModelTitle = "Игровое лобби";

        ChallengePlayerCommand = new AsyncRelayCommand<PlayerSelectableItem>(ChallengePlayerExecuteAsync);
        UpdateSettingsCommand = new AsyncRelayCommand(SaveSettingsCommandExecuteAsync);
        AcceptChallengeCommand = new AsyncRelayCommand(AcceptChallengeCommandExecuteAsync);
        RejectChallengeCommand = new AsyncRelayCommand(RejectChallengeCommandExecuteAsync);

        _navigationService = navigationService;
        _gameAPI = gameAPI;
        _signalRServer = signalRServer;

        _signalRServer.OnNewPlayerConnectAction += OnPlayerConnect;
        _signalRServer.OnPlayerDisconnectAction += OnPlayerDisconnect;

        _signalRServer.OnPlayerUpdateSettingsAction += OnPlayerUpdateSettings;

        _signalRServer.OnPlayerSendChallengeAction += OnPlayerSendChallenge;
        _signalRServer.OnPlayerReceiveChallengeAction += OnPlayerReceiveChallenge;
        _signalRServer.OnPlayerUndoChallengeAction += OnPlayerCancelChallenge;
        _signalRServer.OnPlayerRejectChallengeAction += OnPlayerRejectChallenge;
    }

    #region Properties

    public string CurrentPlayerName { get; private set; }

    public ObservableCollection<PlayerSelectableItem> Players { get; private set; } = [];

    public ObservableCollection<GridToPlayType> GridTypes { get; } = [GridToPlayType.Default, GridToPlayType.Diamond];

    public ObservableCollection<GridToPlaySize> GridSizes { get; } = [GridToPlaySize.ThreeToThree, GridToPlaySize.FiveToFive, GridToPlaySize.SixToSix];

    public GridToPlayType SelectedGridType
    {
        get => _selectedGridType;
        set
        {
            if (_selectedGridType == value)
            {
                return;
            }

            _selectedGridType = value;
            OnPropertyChanged();

            // Reset selected grid size despite matching types.
            SelectedGridSize = GridToPlaySize.ThreeToThree;
    
            // For now only third types are different.
            GridSizes[2] = value is GridToPlayType.Default ? GridToPlaySize.SixToSix : GridToPlaySize.SevenToSeven;
            OnPropertyChanged(nameof(GridSizes));
        }
    }

    #endregion

    #region Commands

    public ICommand ChallengePlayerCommand { get; }

    public ICommand UpdateSettingsCommand { get; }

    public ICommand AcceptChallengeCommand { get; }

    public ICommand RejectChallengeCommand { get; }

    #endregion

    #region Methods

    public override async Task<NavigationResult> OnNavigatedToAsync(NavigationArgs args)
    { 
        CurrentPlayerName = args.Parameters.GetValue<string>("FirstPlayerName");

        await LoadConnectedPlayersAsync().ConfigureAwait(false);
        return await base.OnNavigatedToAsync(args).ConfigureAwait(false);
    }

    private async Task LoadConnectedPlayersAsync()
    {
        var connectedPlayers = await _gameAPI.GetConnectedPlayers().ConfigureAwait(false);
        await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                    {
                                                                        Players = new(connectedPlayers.Where(x => x.Name != CurrentPlayerName).Select(x => new PlayerSelectableItem(x)));
                                                                        OnPropertyChanged(nameof(Players));
                                                                    });
    }

    #region ServerEventHandlers

    private void OnPlayerConnect(Player player)
    {
        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             Players.Add(new PlayerSelectableItem(player));
                                                             OnPropertyChanged(nameof(Players));
                                                         });
    }

    private void OnPlayerDisconnect(string disconnectedPlayerName)
    {
        var disconnectedSelectablePlayer = Players.FirstOrDefault(x => x.Name == disconnectedPlayerName);
        if (disconnectedSelectablePlayer is null)
        {
            return;
        }

        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             Players.Remove(disconnectedSelectablePlayer);
                                                             OnPropertyChanged(nameof(Players));
                                                         });
    }

    private void OnPlayerUpdateSettings(Player updatedPlayer)
    {
        var playerToUpdate = Players.FirstOrDefault(x => x.Name == updatedPlayer.Name);
        if (playerToUpdate is null)
        {
            return;
        }

        playerToUpdate.Status = updatedPlayer.Status;
        playerToUpdate.PreferredGridType = updatedPlayer.Settings.GridToPlayType;
        playerToUpdate.PreferredGridSize = updatedPlayer.Settings.GridToPlaySize;
    }

    private void OnPlayerSendChallenge(string challengeSenderName)
    {
        ReceiveChallenge = true;
        ChallengeSenderName = challengeSenderName;
        ChallengeMessage = $"Приглашение на совместную игру от {ChallengeSenderName}";
    }

    private void OnPlayerReceiveChallenge(string challengedPlayerName)
    {
        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             var challengedPlayer = Players.FirstOrDefault(x => x.Name == challengedPlayerName);
                                                             if (challengedPlayer is null)
                                                             {
                                                                 return;
                                                             }

                                                             challengedPlayer.Status = PlayerStatus.Challenged;
                                                         });
    }

    private void OnPlayerCancelChallenge(string challengeCancelerName)
    {
        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             if (ReceiveChallenge)
                                                             {
                                                                 ReceiveChallenge = false;
                                                                 _ = MessageBox.Show($"Игрок {challengeCancelerName} отменил приглашение на совместную игру.", MsgBoxButton.OK, MsgBoxImage.Information);
                                                             }
                                                             else
                                                             {
                                                                 var challengedPlayer = Players.FirstOrDefault(x => x.Name == challengeCancelerName);
                                                                 if (challengedPlayer is null)
                                                                 {
                                                                     return;
                                                                 }

                                                                 challengedPlayer.Status = PlayerStatus.FreeToPlay;
                                                             }
                                                         });
    }

    private void OnPlayerRejectChallenge(string challengeRejectorName)
    {
        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             foreach (var player in Players)
                                                             {
                                                                 ProcessPlayerRejection(player, challengeRejectorName);
                                                             }
                                                         });
    }

    private static void ProcessPlayerRejection(PlayerSelectableItem player, string challengeRejectorName)
    {
        // Deal with player who was challenged by the current player.
        // We can identify it properly using 'WasChallenged' property,
        // because only one player at a time can be in a challenged state (status + spinner).
        if (player.Name == challengeRejectorName && player.WasChallenged)
        {
            player.WasChallenged = false;
            _ = MessageBox.Show($"Игрок {challengeRejectorName} отклонил запрос на совместную игру.", MsgBoxButton.OK, MsgBoxImage.Information);
        }
        // Deal with other players.
        else
        {
            // Don't touch players who are playing or not available right now.
            if (player.Status is not (PlayerStatus.Challenged or PlayerStatus.FreeToPlay))
            {
                return;
            }

            // For the others we can safely set the status to 'FreeToPlay',
            // to cause the 'CanBeChallenged' property to be updated.
            player.Status = PlayerStatus.FreeToPlay;
        }
    }

    #endregion

    #region CommandMethods

    private async Task ChallengePlayerExecuteAsync(PlayerSelectableItem challengedPlayer)
    {
        try
        {
            // If a challenge has been made, cancel the challenge and vice versa.
            challengedPlayer.WasChallenged = !challengedPlayer.WasChallenged;

            foreach (var otherPlayer in Players)
            {
                // Skip challenged player.
                if (otherPlayer == challengedPlayer)
                {
                    continue;
                }

                // Disable the ability to send a challenge to other players.
                otherPlayer.CanBeChallenged = !challengedPlayer.WasChallenged;
            }

            if (challengedPlayer.WasChallenged)
            {
                await _signalRServer.SendChallengeAsync(challengedPlayer.Name).ConfigureAwait(false);
            }
            else
            {
                await _signalRServer.UndoChallengeAsync(challengedPlayer.Name).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                        {
                                                                            _ = MessageBox.Show("Не удалось отправить запрос на совместную игру.", MsgBoxButton.OK, MsgBoxImage.Error);
                                                                        });
        }
    }

    private async Task SaveSettingsCommandExecuteAsync()
    {
        try
        {
            var newSettings = new SettingsHolder
            {
                DoNotDisturb = DoNotDisturb,
                GridToPlayType = SelectedGridType,
                GridToPlaySize = SelectedGridSize
            };

            if (_currentSettings == newSettings)
            {
                return;
            }

            _currentSettings = newSettings;

            SettingsUpdateStarted = true;

            await Task.Delay(1500).ConfigureAwait(false);
            await _signalRServer.SendPlayerUpdateSettingsAsync(newSettings).ConfigureAwait(false);

            SettingsUpdateStarted = false;
        }
        catch (Exception ex)
        {
            SettingsUpdateStarted = false;
            await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                        {
                                                                            _ = MessageBox.Show("Не удалось обновить настройки.", MsgBoxButton.OK, MsgBoxImage.Error);
                                                                        });
        }
    }

    private async Task AcceptChallengeCommandExecuteAsync()
    {
        try
        {
            ReceiveChallenge = false;
            await _signalRServer.SendChallengeAnswerAsync(true, ChallengeSenderName).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // Console.WriteLine(e);
            // throw;
        }
    }

    private async Task RejectChallengeCommandExecuteAsync()
    {
        try
        {
            ReceiveChallenge = false;
            await _signalRServer.SendChallengeAnswerAsync(false, ChallengeSenderName).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // Console.WriteLine(e);
            // throw;
        }
    }

    #endregion

    public override void Dispose()
    {
        _signalRServer.OnNewPlayerConnectAction -= OnPlayerConnect;
        _signalRServer.OnPlayerUpdateSettingsAction -= OnPlayerUpdateSettings;
        _signalRServer.OnPlayerSendChallengeAction -= OnPlayerSendChallenge;
        _signalRServer.OnPlayerRejectChallengeAction -= OnPlayerRejectChallenge;
        _signalRServer.OnPlayerDisconnectAction -= OnPlayerDisconnect;
    }

    #endregion
}