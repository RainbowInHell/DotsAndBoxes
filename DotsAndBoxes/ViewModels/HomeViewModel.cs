using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using DotsAndBoxes.SignalR;
using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesUIComponents;
using DotsAndBoxesServerAPI.Refit;
using MaterialDesignThemes.Wpf;

namespace DotsAndBoxes.ViewModels;

[Route(Routes.Home)]
public partial class HomeViewModel : BaseViewModel
{
    #region Fields

    private readonly INavigationService<BaseViewModel> _navigationService;

    private readonly IGameAPI _gameAPI;

    private readonly SignalRServer _signalRServer;

    [ObservableProperty]
    private ObservableCollection<GameTypeSelectableItem> _gameTypes;

    private GameTypeSelectableItem _selectedGameTypeItem;

    [ObservableProperty]
    private string _firstPlayerName;

    [ObservableProperty]
    private string _secondPlayerName;

    [ObservableProperty]
    private bool _canBeChallenged;

    [ObservableProperty]
    private bool _isLoading;

    #endregion

    public HomeViewModel(INavigationService<BaseViewModel> navigationService, IGameAPI gameAPI, SignalRServer signalRServer)
    {
        _navigationService = navigationService;
        _gameAPI = gameAPI;
        _signalRServer = signalRServer;

        ViewModelTitle = "Домашняя";

        GoToPlayersLobbyCommand = new AsyncRelayCommand(GoToPlayersLobbyExecuteAsync);

        InitializeSelectableGameTypes();
    }

    #region Properties

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

    // TODO: Навигация не только в лобби.
    public ICommand GoToPlayersLobbyCommand { get; }

    #endregion

    #region Methods

    private async Task GoToPlayersLobbyExecuteAsync()
    {
        if (string.IsNullOrEmpty(FirstPlayerName))
        {
            _ = new CustomMessageBox("Имя игрока должно быть заполнено.", MessageType.Error, MessageButtons.Ok).ShowDialog();
            return;
        }

        try
        {
            IsLoading = true;

            // var existedConnectedPlayer = await _gameAPI.GetConnectedPlayerByNameAsync(FirstPlayerName).ConfigureAwait(false);
            // if (existedConnectedPlayer is not null)
            // {
            //     IsLoading = false;
            //     
            //     await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
            //                                                                 {
            //                                                                     _ = new CustomMessageBox("Игрок с таким именем уже есть на сервере.", MessageType.Error, MessageButtons.Ok).ShowDialog();
            //                                                                 });
            //     return;
            // }

            await Task.Delay(1500).ConfigureAwait(false);
            await _signalRServer.StartConnectionAsync().ConfigureAwait(false);

            IsLoading = false;

            var newConnectedPlayer = new Player
            {
                Name = FirstPlayerName,
                CanBeChallenged = CanBeChallenged,
                ConnectionId = _signalRServer.ConnectionId
            };

            await _signalRServer.SendNewPlayerConnectedAsync(newConnectedPlayer).ConfigureAwait(false);
            await _navigationService.NavigateAsync(Routes.PlayersLobby).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            IsLoading = false;
            await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                        {
                                                                            _ = new CustomMessageBox($"Сервер недоступен, попробуйте позже.\n{ex}", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                                                        });
        }
    }

    private void InitializeSelectableGameTypes()
    {
        GameTypes =
        [
            new GameTypeSelectableItem
            {
                Icon = PackIconKind.Account,
                Name = "Одиночный"
            },

            new GameTypeSelectableItem
            {
                Icon = PackIconKind.PersonMultiple,
                Name = "Вдвоем"
            },

            new GameTypeSelectableItem
            {
                Icon = PackIconKind.LanConnect,
                Name = "По сети"
            }
        ];

        SelectedGameTypeItem = GameTypes[0];
    }

    #endregion
}

public class GameTypeSelectableItem
{
    public PackIconKind Icon { get; init; }

    public required string Name { get; init; }
}