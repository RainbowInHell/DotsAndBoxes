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
using Microsoft.AspNetCore.SignalR.Client;

namespace DotsAndBoxes.ViewModels;

[Route(Routes.PlayersLobby)]
public sealed partial class PlayersLobbyViewModel : BaseViewModel, IDisposable
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

    private readonly SignalRClient _signalRClient;

    #endregion

    public PlayersLobbyViewModel(INavigationService<BaseViewModel> navigationService, IGameAPI gameAPI, SignalRClient signalRClient)
    {
        ViewModelTitle = "Игровое лобби";

        ChallengePlayerCommand = new AsyncRelayCommand<PlayerSelectableItem>(ChallengePlayerExecuteAsync);
        UpdateSettingsCommand = new AsyncRelayCommand(SaveSettingsCommandExecuteAsync);
        AcceptChallengeCommand = new AsyncRelayCommand(AcceptChallengeCommandExecuteAsync);
        RejectChallengeCommand = new AsyncRelayCommand(RejectChallengeCommandExecuteAsync);

        _navigationService = navigationService;
        _gameAPI = gameAPI;
        _signalRClient = signalRClient;

        _signalRClient.OnPlayerConnectAction += OnPlayerConnect;
        _signalRClient.OnPlayerDisconnectAction += OnPlayerDisconnect;
        _signalRClient.OnPlayerUpdateSettingsAction += OnPlayerUpdateSettings;
        _signalRClient.OnPlayerChangeStatusAction += OnPlayerChangeStatus;

        _signalRClient.OnChallengeAction += OnChallenge;
        _signalRClient.OnChallengeCancelAction += OnChallengeCancel;
        _signalRClient.OnChallengeRejectAction += OnChallengeReject;
        _signalRClient.OnChallengeAcceptAction += OnChallengeAccept;

        _signalRClient.OnConnectionStateChangedAction += OnServerConnectionStateChanged;
        _signalRClient.OnRetry += OnRetry;
    }

    [ObservableProperty]
    private uint _reconnectAttemptsCount;

    private void OnRetry()
    {
        ++ReconnectAttemptsCount;
    }

    [ObservableProperty]
    private bool _connectionLost;

    private void OnServerConnectionStateChanged(HubConnectionState newConnectionState)
    {
        ConnectionLost = newConnectionState switch
        {
            HubConnectionState.Reconnecting => true,
            HubConnectionState.Connected => false,
            _ => ConnectionLost
        };
    }

    #region Properties

    public override bool DisposeOnNavigate => true;

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

    #region StateEvents

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

    #endregion

    /// <summary>
    /// Occurs when current player receive challenge.
    /// </summary>
    /// <param name="challengeSenderName">Challenge sender name.</param>
    private void OnChallenge(string challengeSenderName)
    {
        ReceiveChallenge = true;
        ChallengeSenderName = challengeSenderName;
        ChallengeMessage = $"Приглашение на совместную игру от {ChallengeSenderName}";
    }

    /// <summary>
    /// Occurs when someone of the connected players was challenged.
    /// </summary>
    /// <param name="playerName">Player which status was changed.</param>
    /// <param name="newStatus">New status.</param>
    private void OnPlayerChangeStatus(string playerName, PlayerStatus newStatus)
    {
        var existedPlayer = Players.FirstOrDefault(x => x.Name == playerName);
        if (existedPlayer is null)
        {
            return;
        }

        existedPlayer.Status = newStatus;
    }

    /// <summary>
    /// Occurs when challenge sender for current player cancel the challenge.
    /// </summary>
    private void OnChallengeCancel()
    {
        ReceiveChallenge = false;
        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                          {
                                                              _ = MessageBox.Show($"Игрок {ChallengeSenderName} отменил приглашение.", MsgBoxButton.OK, MsgBoxImage.Information);
                                                          });
    }

    private void OnChallengeReject()
    {
        var challengedPlayer = Players.First(x => x.WasChallenged);
        challengedPlayer.WasChallenged = false;

        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             _ = MessageBox.Show($"Игрок {challengedPlayer.Name} отклонил приглашение.", MsgBoxButton.OK, MsgBoxImage.Information);
                                                         });
    }

    private void OnChallengeAccept(string challengedPlayerName)
    {
        ReceiveChallenge = false;

        var a = challengedPlayerName;
        var a1 = CurrentPlayerName;
        var a2 = ChallengeSenderName;
        _navigationService.Navigate(Routes.Game, new DynamicDictionary((nameof(CurrentPlayerName), CurrentPlayerName),
                                                                       (nameof(ChallengeSenderName), ChallengeSenderName),
                                                                       (nameof(SelectedGridType), SelectedGridType),
                                                                       (nameof(SelectedGridSize), SelectedGridSize)));
    }

    #endregion

    #region CommandMethods

    private async Task ChallengePlayerExecuteAsync(PlayerSelectableItem challengedPlayer)
    {
        try
        {
            if (!challengedPlayer.WasChallenged && Players.Any(x => x.WasChallenged))
            {
                _ = MessageBox.Show("Дождитесь ответа на предыдущее приглашение, либо отклоните его, чтобы отправить другое.");
                return;
            }

            // If a challenge has been made, cancel the challenge and vice versa.
            challengedPlayer.WasChallenged = !challengedPlayer.WasChallenged;

            if (challengedPlayer.WasChallenged)
            {
                await _signalRClient.SendChallengeAsync(challengedPlayer.Name).ConfigureAwait(false);
            }
            else
            {
                await _signalRClient.UndoChallengeAsync(challengedPlayer.Name).ConfigureAwait(false);
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
            await _signalRClient.SendPlayerUpdateSettingsAsync(newSettings).ConfigureAwait(false);

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
            await _signalRClient.SendChallengeAnswerAsync(true, ChallengeSenderName).ConfigureAwait(false);
            _navigationService.Navigate(Routes.Game, new DynamicDictionary((nameof(CurrentPlayerName), CurrentPlayerName),
                                                                           (nameof(ChallengeSenderName), ChallengeSenderName),
                                                                           (nameof(SelectedGridType), SelectedGridType),
                                                                           (nameof(SelectedGridSize), SelectedGridSize)));
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
            await _signalRClient.SendChallengeAnswerAsync(false, ChallengeSenderName).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // Console.WriteLine(e);
            // throw;
        }
    }

    #endregion

    public void Dispose()
    {
        _signalRClient.OnPlayerConnectAction -= OnPlayerConnect;
        _signalRClient.OnPlayerDisconnectAction -= OnPlayerDisconnect;
        _signalRClient.OnPlayerUpdateSettingsAction -= OnPlayerUpdateSettings;
        _signalRClient.OnPlayerChangeStatusAction -= OnPlayerChangeStatus;

        _signalRClient.OnChallengeAction -= OnChallenge;
        _signalRClient.OnChallengeCancelAction -= OnChallengeCancel;
        _signalRClient.OnChallengeRejectAction -= OnChallengeReject;
        _signalRClient.OnChallengeAcceptAction -= OnChallengeAccept;

        _signalRClient.OnConnectionStateChangedAction -= OnServerConnectionStateChanged;
        _signalRClient.OnRetry -= OnRetry;
    }

    #endregion
}