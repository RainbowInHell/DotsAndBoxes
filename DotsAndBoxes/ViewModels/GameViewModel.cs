using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using DotsAndBoxes.SignalR;
using DotsAndBoxesUIComponents;

namespace DotsAndBoxes.ViewModels;

[Route(Routes.Game)]
public sealed partial class GameViewModel : BaseViewModel, IDisposable
{
    private readonly SignalRClient _signalRClient;

    private readonly GameController _gameController;

    private readonly Brush _firstPlayerColor = Brushes.LawnGreen;

    private readonly Brush _secondPlayerColor = Brushes.OrangeRed;

    [ObservableProperty]
    private bool _canMakeMove = true;

    [ObservableProperty]
    private int _currentPlayerScore;

    [ObservableProperty]
    private int _opponentPlayerScore;

    private readonly INavigationService<BaseViewModel> _navigationService;

    public GameViewModel(SignalRClient signalRClient, INavigationService<BaseViewModel> navigationService)
    {
        ViewModelTitle = "Игра";

        _signalRClient = signalRClient;
        _navigationService = navigationService;
        _gameController = new GameController();

        _signalRClient.OnOpponentMakeMove += OnOpponentMakeMove;
        _signalRClient.OnOpponentWinGame += OnOpponentWinGame;
        _signalRClient.OnOpponentLeaveGame += OnOpponentLeaveGame;
        _signalRClient.OnConnectionLost += OnConnectionLost;

        Lines = new(_gameController.LineList);
        Dots = new(_gameController.PointList);

        ClickLineCommand = new AsyncRelayCommand<DrawableLine>(ClickLineCommandExecuteAsync, ClickLineCommandCanExecute);
        LeaveGameCommand = new AsyncRelayCommand(LeaveGameCommandExecuteAsync);
    }

    #region Properties

    public ObservableCollection<DrawableLine> Lines { get; }

    public ObservableCollection<DrawablePoint> Dots { get; }

    public string FirstPlayerName { get; private set; }

    public string SecondPlayerName { get; private set; }

    #endregion

    #region Commands

    public ICommand ClickLineCommand { get; }

    public ICommand LeaveGameCommand { get; }

    #endregion

    #region Methods

    public override NavigationResult OnNavigatedTo(NavigationArgs args)
    {
        FirstPlayerName = args.Parameters.GetValue<string>(nameof(FirstPlayerName));
        SecondPlayerName = args.Parameters.GetValue<string>(nameof(SecondPlayerName));
        CanMakeMove = args.Parameters.GetValue<bool>("CanMakeMove");

        return base.OnNavigatedTo(args);
    }

    #region CommandMethods

    private async Task ClickLineCommandExecuteAsync(DrawableLine line)
    {
        // Dummy switch to the thread pool.
        await Task.Delay(1).ConfigureAwait(false);

        line.Color = _firstPlayerColor;

        if (_gameController.IsSquareCompleted(line))
        {
            CanMakeMove = true;
            ++CurrentPlayerScore;
        }
        else
        {
            CanMakeMove = false;
        }

        await _signalRClient.MakeMoveAsync(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y).ConfigureAwait(false);
        if (_gameController.IsGameEnded())
        {
            await _signalRClient.EndGameAsync().ConfigureAwait(false);

            await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                        {
                                                                            MessageBox.Show("Игра окончена!", MsgBoxButton.OK, MsgBoxImage.Information);
                                                                        });
            await _navigationService.NavigateAsync(Routes.PlayersLobby,
                                                   new DynamicDictionary(("FirstPlayerName", FirstPlayerName))).ConfigureAwait(false);
        }
    }

    private bool ClickLineCommandCanExecute(DrawableLine drawable)
    {
        return CanMakeMove && drawable.Color != _secondPlayerColor;
    }

    private async Task LeaveGameCommandExecuteAsync()
    {
        await _signalRClient.LeaveGameAsync().ConfigureAwait(false);
        await _navigationService.NavigateAsync(Routes.PlayersLobby,
                                               new DynamicDictionary(("FirstPlayerName", FirstPlayerName))).ConfigureAwait(false);
    }

    #endregion

    #region ConnectionEventHandlers

    private void OnConnectionLost()
    {
        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             MessageBox.Show("Соединение с сервером потеряно.\nТекущая сессия будет завершена.", MsgBoxButton.OK, MsgBoxImage.Warning);
                                                         });

        _signalRClient.StopConnectionAsync().SafeFireAndForget();
        _navigationService.Navigate(Routes.Home);
    }

    #endregion
    
    #region GameEventHandlers

    private void OnOpponentMakeMove(int x1, int y1, int x2, int y2)
    {
        var line = Lines.First(x => x.StartPoint.X == x1 && x.StartPoint.Y == y1 && x.EndPoint.X == x2 && x.EndPoint.Y == y2);
        line.Color = _secondPlayerColor;

        if(_gameController.IsSquareCompleted(line))
        {
            CanMakeMove = false;
            ++OpponentPlayerScore;
        }
        else
        {
            CanMakeMove = true;
        }
    }

    private async Task OnOpponentWinGame()
    {
        await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                    {
                                                                        MessageBox.Show("Игра окончена!", MsgBoxButton.OK, MsgBoxImage.Information);
                                                                    });

        await _navigationService.NavigateAsync(Routes.PlayersLobby,
                                               new DynamicDictionary(("FirstPlayerName", FirstPlayerName))).ConfigureAwait(false);
    }

    private async Task OnOpponentLeaveGame()
    {
        await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                    {
                                                                        MessageBox.Show("Соперник покинул игру. Текущая сессия будет завершена.", MsgBoxButton.OK, MsgBoxImage.Information);
                                                                    });
        await _navigationService.NavigateAsync(Routes.PlayersLobby,
                                               new DynamicDictionary(("FirstPlayerName", FirstPlayerName))).ConfigureAwait(false);
    }

    #endregion

    public void Dispose()
    {
        _signalRClient.OnOpponentMakeMove -= OnOpponentMakeMove;
        _signalRClient.OnOpponentWinGame += OnOpponentWinGame;
        _signalRClient.OnOpponentLeaveGame -= OnOpponentLeaveGame;
        _signalRClient.OnConnectionLost -= OnConnectionLost;
    }

    #endregion
}