using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using DotsAndBoxes.SelectableItems;
using DotsAndBoxes.SignalR;
using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesUIComponents;
using DotsAndBoxesServerAPI.Refit;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Logging;
using MessageBox = DotsAndBoxesUIComponents.MessageBox;

namespace DotsAndBoxes.ViewModels;

[Route(Routes.Home)]
public partial class HomeViewModel : BaseViewModel
{
    #region Fields

    private GameTypeSelectableItem _selectedGameTypeItem;

    #region FieldsWithObservableProperties

    [ObservableProperty]
    private string _firstPlayerName;

    [ObservableProperty]
    private string _secondPlayerName;

    [ObservableProperty]
    private bool _doNotDisturb;

    [ObservableProperty]
    private bool _isLoading;

    #endregion

    private readonly INavigationService<BaseViewModel> _navigationService;

    private readonly IGameAPI _gameAPI;

    private readonly SignalRServer _signalRServer;

    private readonly ILogger<HomeViewModel> _logger;

    #endregion

    public HomeViewModel(ILogger<HomeViewModel> logger,
                         INavigationService<BaseViewModel> navigationService,
                         IGameAPI gameAPI,
                         SignalRServer signalRServer)
    {
        ViewModelTitle = "Домашняя";
        SelectedGameTypeItem = GameTypes[0];

        GoToPlayersLobbyCommand = new AsyncRelayCommand(GoToPlayersLobbyExecuteAsync);

        _logger = logger;
        _navigationService = navigationService;
        _gameAPI = gameAPI;
        _signalRServer = signalRServer;
    }

    #region Properties

    public ObservableCollection<GameTypeSelectableItem> GameTypes { get; } =
    [
        // new GameTypeSelectableItem
        // {
        //     Icon = PackIconKind.Account,
        //     Name = "Одиночный"
        // },
        //
        // new GameTypeSelectableItem
        // {
        //     Icon = PackIconKind.PersonMultiple,
        //     Name = "Вдвоем"
        // },
        new()
        {
            Icon = PackIconKind.LanConnect,
            Name = "По сети"
        }
    ];

    public PackIconKind SelectedGameType => SelectedGameTypeItem.Icon;

    public GameTypeSelectableItem SelectedGameTypeItem
    {
        get => _selectedGameTypeItem;
        set
        {
            if (_selectedGameTypeItem == value)
            {
                return;
            }

            _selectedGameTypeItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedGameType));
        }
    }

    #endregion

    #region Commands

    public ICommand GoToPlayersLobbyCommand { get; }

    #endregion

    #region Methods

    private async Task GoToPlayersLobbyExecuteAsync()
    {
        if (string.IsNullOrEmpty(FirstPlayerName))
        {
           _ = MessageBox.Show("Имя игрока должно быть заполнено.", MsgBoxButton.OK, MsgBoxImage.Error);
           return;
        }

        try
        {
            IsLoading = true;

            await Task.Delay(1500).ConfigureAwait(false);

            var connectedPlayers = await _gameAPI.GetConnectedPlayers().ConfigureAwait(false);
            if (connectedPlayers.Any(x => x.Name.Equals(FirstPlayerName, StringComparison.OrdinalIgnoreCase)))
            {
                IsLoading = false;
                
                await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                            {
                                                                                _ = MessageBox.Show("Игрок с таким именем уже есть на сервере.", MsgBoxButton.OK, MsgBoxImage.Error);
                                                                            });
                return;
            }

            await _signalRServer.StartConnectionAsync().ConfigureAwait(false);

            IsLoading = false;

            var newConnectedPlayer = new Player
            {
                Name = FirstPlayerName,
                Status = DoNotDisturb ? PlayerStatus.DoNotDisturb : PlayerStatus.FreeToPlay
            };

            await _signalRServer.SendNewPlayerConnectAsync(newConnectedPlayer).ConfigureAwait(false);
            await _navigationService.NavigateAsync(Routes.PlayersLobby,
                                                   new DynamicDictionary((nameof(FirstPlayerName), FirstPlayerName))).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("Can't go to players lobby due to an error: {ex}", ex);

            IsLoading = false;
            await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                        {
                                                                            _ = MessageBox.Show("Сервер недоступен, попробуйте позже.", MsgBoxButton.OK, MsgBoxImage.Error);
                                                                        });
        }
    }

    #endregion
}
