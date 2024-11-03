using System.Collections.ObjectModel;
using System.Windows.Input;
using AsyncAwaitBestPractices;
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
public sealed partial class PlayersLobbyViewModel : BaseViewModel, IDisposable
{
    #region Fields

    private SettingsHolder _currentSettings;

    private string _lastChallengedPlayer;

    #region FieldsWithObservableProperties

    [ObservableProperty]
    private GridToPlayType _selectedGridType = GridToPlayType.Default;

    [ObservableProperty]
    private GridSize _selectedGridSize = GridSize.ThreeToThree;

    [ObservableProperty]
    private bool _doNotDisturb;

    [ObservableProperty]
    private bool _settingsUpdateStarted;

    [ObservableProperty]
    private bool _receiveChallenge;

    [ObservableProperty]
    private string _challengeSenderName;

    [ObservableProperty]
    private bool _connectionIsLost;

    [ObservableProperty]
    private long _reconnectAttemptsCount;

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

        _signalRClient.OnPlayerConnect += OnPlayerConnect;
        _signalRClient.OnPlayerDisconnect += OnPlayerDisconnect;
        _signalRClient.OnPlayerUpdateSettings += OnPlayerUpdateSettings;
        _signalRClient.OnPlayerChangeStatus += OnPlayerChangeStatus;

        _signalRClient.OnChallenge += OnChallenge;
        _signalRClient.OnChallengeCancel += OnChallengeCancel;
        _signalRClient.OnChallengeReject += OnChallengeReject;
        _signalRClient.OnChallengeAccept += OnChallengeAccept;

        _signalRClient.OnConnectionLost += OnConnectionLost;
        _signalRClient.OnReconnectAttempt += OnReconnectAttempt;
        _signalRClient.OnConnectionRestored += OnConnectionRestored;
    }

    #region Properties

    public string CurrentPlayerName { get; private set; }

    public ObservableCollection<PlayerSelectableItem> Players { get; private set; } = [];

    public ObservableCollection<GridToPlayType> GridTypes { get; } = [GridToPlayType.Default];

    public ObservableCollection<GridSize> GridSizes { get; } = [GridSize.ThreeToThree, GridSize.FiveToFive, GridSize.SixToSix];

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

        var connectedPlayers = await _gameAPI.GetConnectedPlayers().ConfigureAwait(false);
        await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                    {
                                                                        Players = new(connectedPlayers.Where(x => x.Name != CurrentPlayerName).Select(x => new PlayerSelectableItem(x)));
                                                                        OnPropertyChanged(nameof(Players));
                                                                    });

        return await base.OnNavigatedToAsync(args).ConfigureAwait(false);
    }

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
                _lastChallengedPlayer = challengedPlayer.Name;
                await _signalRClient.SendChallengeAsync(challengedPlayer.Name).ConfigureAwait(false);
            }
            else
            {
                _lastChallengedPlayer = null;
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
                GridSize = SelectedGridSize
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
            await _signalRClient.SendChallengeAnswerAsync(true, ChallengeSenderName).ConfigureAwait(false);
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

    #region PlayerEventHandlers

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
        playerToUpdate.PreferredGridSize = updatedPlayer.Settings.GridSize;
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

    #endregion

    #region ChallengeEventHandlers

    /// <summary>
    /// Occurs when current player receive challenge.
    /// </summary>
    /// <param name="challengeSenderName">Challenge sender name.</param>
    private void OnChallenge(string challengeSenderName)
    {
        ReceiveChallenge = true;
        ChallengeSenderName = challengeSenderName;
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

        _lastChallengedPlayer = challengedPlayer.Name;

        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             _ = MessageBox.Show($"Игрок {challengedPlayer.Name} отклонил приглашение.", MsgBoxButton.OK, MsgBoxImage.Information);
                                                         });
    }

    private void OnChallengeAccept(string lobbyId)
    {
        if (ReceiveChallenge)
        {
            ReceiveChallenge = false;
            _navigationService.Navigate(Routes.Game, new DynamicDictionary(("FirstPlayerName", CurrentPlayerName),
                                                                           ("SecondPlayerName", ChallengeSenderName),
                                                                           ("CanMakeMove", true),
                                                                           ("LobbyId", lobbyId)));
        }
        else
        {
            _navigationService.Navigate(Routes.Game, new DynamicDictionary(("FirstPlayerName", CurrentPlayerName),
                                                                           ("SecondPlayerName", _lastChallengedPlayer),
                                                                           ("CanMakeMove", false),
                                                                           ("LobbyId", lobbyId)));
        }
    }

    #endregion

    #region ConnectionEventHandlers

    private void OnConnectionLost()
    {
        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             ConnectionIsLost = true;
                                                             Players.Clear();
                                                         });
    }

    private void OnReconnectAttempt(long attemptsCount)
    {
        if (attemptsCount != SignalRClient.MaxReconnectAttempts)
        {
            ReconnectAttemptsCount++;
            return;
        }

        ConnectionIsLost = false;
        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             MessageBox.Show("Не удалось восстановить подключение к серверу.\nТекущая сессия будет завершена.", MsgBoxButton.OK, MsgBoxImage.Warning);
                                                         });

        _navigationService.Navigate(Routes.Home);
    }

    private void OnConnectionRestored()
    {
        ConnectionIsLost = false;

        var newConnectedPlayer = new Player
        {
            Name = CurrentPlayerName,
            Status = DoNotDisturb ? PlayerStatus.DoNotDisturb : PlayerStatus.FreeToPlay
        };

        _signalRClient.SendNewPlayerConnectAsync(newConnectedPlayer).SafeFireAndForget();
    }

    #endregion

    public void Dispose()
    {
        _signalRClient.OnPlayerConnect -= OnPlayerConnect;
        _signalRClient.OnPlayerDisconnect -= OnPlayerDisconnect;
        _signalRClient.OnPlayerUpdateSettings -= OnPlayerUpdateSettings;
        _signalRClient.OnPlayerChangeStatus -= OnPlayerChangeStatus;

        _signalRClient.OnChallenge -= OnChallenge;
        _signalRClient.OnChallengeCancel -= OnChallengeCancel;
        _signalRClient.OnChallengeReject -= OnChallengeReject;
        _signalRClient.OnChallengeAccept -= OnChallengeAccept;

        _signalRClient.OnConnectionLost -= OnConnectionLost;
        _signalRClient.OnReconnectAttempt -= OnReconnectAttempt;
        _signalRClient.OnConnectionRestored -= OnConnectionRestored;
    }

    #endregion
}