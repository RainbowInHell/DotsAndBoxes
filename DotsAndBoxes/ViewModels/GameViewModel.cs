using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using DotsAndBoxes.SignalR;
using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesUIComponents;
using MessageBox = DotsAndBoxesUIComponents.MessageBox;

namespace DotsAndBoxes.ViewModels;

[Route(Routes.Game)]
public sealed partial class GameViewModel : BaseViewModel, IDisposable
{
    private AiPlayer _aiPlayer;

    private bool _isAgainstAi;

    private readonly SignalRClient _signalRClient;

    private GameController _gameController;

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

        _signalRClient.OnOpponentMakeMove += OnOpponentMakeMove;
        _signalRClient.OnOpponentWinGame += OnOpponentWinGame;
        _signalRClient.OnOpponentLeaveGame += OnOpponentLeaveGame;
        _signalRClient.OnConnectionLost += OnConnectionLost;

        ClickLineCommand = new AsyncRelayCommand<DrawableLine>(ClickLineCommandExecuteAsync, ClickLineCommandCanExecute);
        LeaveGameCommand = new AsyncRelayCommand(LeaveGameCommandExecuteAsync);
    }

    #region Properties

    public ObservableCollection<DrawableLine> Lines { get; private set; }

    public ObservableCollection<DrawablePoint> Dots { get; private set; }

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
        var selectedGridSize = args.Parameters.GetValue<GridToPlaySize>("GridSize");

        _gameController = new GameController(selectedGridSize);
        Lines = new(_gameController.LineList);
        Dots = new(_gameController.PointList);

        if (SecondPlayerName == "Компьютер")
        {
            _isAgainstAi = true;
            _aiPlayer = new AiPlayer(_gameController, _secondPlayerColor);
        }

        return base.OnNavigatedTo(args);
    }

    #region CommandMethods

    // private async Task ClickLineCommandExecuteAsync(DrawableLine line)
    // {
    //     line.Color = _firstPlayerColor;
    //     CanMakeMove = false;
    //
    //     var points = _gameController.MakeMove(line);
    //     if (points > 0)
    //     {
    //         CurrentPlayerScore += points;
    //         CanMakeMove = true;
    //     }
    //     else
    //     {
    //         if (_isAgainstAi)
    //         {
    //             ExecuteAiTurn();
    //         }
    //     }
    //
    //     if (!_isAgainstAi)
    //     {
    //         await _signalRClient.MakeMoveAsync(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
    //     }
    //
    //     if (_gameController.IsGameEnded())
    //     {
    //         await EndGameAsync();
    //     }
    // }
    private async Task ClickLineCommandExecuteAsync(DrawableLine line)
    {
        line.Color = _firstPlayerColor;
        CanMakeMove = false;

        var points = _gameController.MakeMove(line);
        if (points > 0)
        {
            CurrentPlayerScore += points;
            CanMakeMove = true;
        }
        else if (_isAgainstAi)
        {
            await ExecuteAiTurnAsync().ConfigureAwait(false);
        }

        if (!_isAgainstAi)
        {
            await _signalRClient.MakeMoveAsync(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
        }

        if (_gameController.IsGameEnded())
        {
            await EndGameAsync();
        }
    }

    private async Task ExecuteAiTurnAsync()
    {
        int pointsGained;
        do
        {
            pointsGained = _aiPlayer.MakeMove(Lines);
            OpponentPlayerScore += pointsGained;

            if (pointsGained > 0 && !_gameController.IsGameEnded())
            {
                await Task.Delay(2000).ConfigureAwait(false);
            }
        } while (pointsGained > 0);

        CanMakeMove = true;
    }

    private async Task EndGameAsync()
    {
        await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                    {
                                                                        MessageBox.Show("Игра окончена!", MsgBoxButton.OK, MsgBoxImage.Information);
                                                                    });

        if (_isAgainstAi)
        {
            await _navigationService.NavigateAsync(Routes.Home).ConfigureAwait(false);
            return;
        }

        await _navigationService.NavigateAsync(Routes.PlayersLobby, new DynamicDictionary(("FirstPlayerName", FirstPlayerName)))
                                .ConfigureAwait(false);
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

        var points = _gameController.MakeMove(line);
        if(points > 0)
        {
            CanMakeMove = false;
            OpponentPlayerScore += points;
        }
        else
        {
            CanMakeMove = true;
        }

        if (_gameController.IsGameEnded())
        {
            EndGameAsync().SafeFireAndForget();
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