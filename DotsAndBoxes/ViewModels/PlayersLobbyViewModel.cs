using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using DotsAndBoxes.SignalR;
using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesUIComponents;

namespace DotsAndBoxes.ViewModels;

[Route(Routes.PlayersLobby)]
public sealed partial class PlayersLobbyViewModel : BaseViewModel
{
    #region Fileds

    private readonly INavigationService<BaseViewModel> _navigationService;

    private readonly SignalRServer _signalRServer;

    [ObservableProperty]
    private ObservableCollection<Player> _players = [];

    #endregion
    
    public PlayersLobbyViewModel(INavigationService<BaseViewModel> navigationService, SignalRServer signalRServer)
    {
        _navigationService = navigationService;
        _signalRServer = signalRServer;

        ViewModelTitle = "Игровое лобби";

        SubscribeOnServerEvents();
    }

    #region Methods

    public override async Task<NavigationResult> OnNavigatedToAsync(NavigationArgs args)
    { 
        await _signalRServer.SendConnectedPlayersActualizationAsync().ConfigureAwait(false);
        return await base.OnNavigatedToAsync(args).ConfigureAwait(false);
    }

    private void SubscribeOnServerEvents()
    {
        _signalRServer.OnNewPlayerConnectedAction += OnNewPlayerConnected;
        _signalRServer.OnConnectedPlayersActualizationAction += OnReceiveConnectedPlayersUpdate;
    }

    private void OnNewPlayerConnected(Player player)
    {
        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             Players.Add(player);
                                                         });
    }

    private void OnReceiveConnectedPlayersUpdate(List<Player> players)
    {
        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             Players = new ObservableCollection<Player>(players);
                                                         });
    }

    #endregion

    public override void Dispose()
    {
        _signalRServer.OnNewPlayerConnectedAction -= OnNewPlayerConnected;
        _signalRServer.OnConnectedPlayersActualizationAction -= OnReceiveConnectedPlayersUpdate;        
    }
}