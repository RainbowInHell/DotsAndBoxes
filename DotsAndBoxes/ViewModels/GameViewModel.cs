using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotsAndBoxesServerAPI;
using DotsAndBoxesUIComponents;
using Microsoft.Extensions.Logging;
using MessageBox = DotsAndBoxesUIComponents.MessageBox;

namespace DotsAndBoxes;

[Route(Routes.Game)]
public sealed partial class GameViewModel : BaseViewModel, IDisposable
{
    #region Fields

    private AiPlayer _aiPlayer;

    private bool _isAgainstAi;

    private string _lobbyId;

    private GameController _gameController;

    private readonly SignalRClient _signalRClient;

    private readonly Brush _firstPlayerColor = Brushes.LawnGreen;

    private readonly Brush _secondPlayerColor = Brushes.OrangeRed;

    private readonly INavigationService<BaseViewModel> _navigationService;

    private readonly ILogger<GameViewModel> _logger;

    #region FieldsWithObservableProperties

    [ObservableProperty]
    private bool _canMakeMove = true;

    [ObservableProperty]
    private int _currentPlayerScore;

    [ObservableProperty]
    private int _opponentPlayerScore;

    #endregion

    #endregion

    public GameViewModel(SignalRClient signalRClient,
                         INavigationService<BaseViewModel> navigationService,
                         ILogger<GameViewModel> logger)
    {
        ViewModelTitle = "Игра";

        _signalRClient = signalRClient;
        _navigationService = navigationService;
        _logger = logger;

        ClickLineCommand = new AsyncRelayCommand<DrawableLine>(ClickLineCommandExecuteAsync, ClickLineCommandCanExecute);
        LeaveGameCommand = new AsyncRelayCommand(LeaveGameCommandExecuteAsync);
    }

    #region Properties

    public ObservableCollection<DrawableLine> Lines { get; private set; }

    public ObservableCollection<Point> Dots { get; private set; }

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
        var selectedGridSize = args.Parameters.GetValue<GridSize>(nameof(GridSize));
        var pointsAndLinesTuple = BoardDrawer.DrawBoard(selectedGridSize);
        Lines = new(pointsAndLinesTuple.lineList);
        Dots = new(pointsAndLinesTuple.pointList);

        FirstPlayerName = args.Parameters.GetValue<string>(nameof(FirstPlayerName));
        SecondPlayerName = args.Parameters.GetValue<string>(nameof(SecondPlayerName));

        if (SecondPlayerName == "Компьютер")
        {
            _isAgainstAi = true;
            _gameController = new GameController(selectedGridSize);
            _aiPlayer = new AiPlayer(_gameController, _secondPlayerColor);
        }
        else
        {
            _signalRClient.OnOpponentMakeMove += OnOpponentMakeMove;
            _signalRClient.OnGainPoints += OnGainPoints;
            _signalRClient.OnGameEnd += OnGameEndAsync;
            _signalRClient.OnOpponentLeaveGame += OnOpponentLeaveGameAsync;
            _signalRClient.OnConnectionLost += OnConnectionLost;

            _lobbyId = args.Parameters.GetValue<string>("LobbyId");
        }

        return base.OnNavigatedTo(args);
    }

    #region CommandMethods

    private async Task ClickLineCommandExecuteAsync(DrawableLine line)
    {
        try
        {
            line.Color = _firstPlayerColor;

            if (_isAgainstAi)
            {
                await ProcessAiLogicAsync(line).ConfigureAwait(false);
            }
            else
            {
                await _signalRClient.MakeMoveAsync(_lobbyId, line.X1, line.Y1, line.X2, line.Y2);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWithCallerInfo(LogLevel.Error, "Can't make move due to an error: ", ex);
            await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                        {
                                                                            _ = MessageBox.Show("Не удалось совершить ход.", MsgBoxButton.OK, MsgBoxImage.Error);
                                                                        });
        }
    }

    private bool ClickLineCommandCanExecute(DrawableLine drawable)
    {
        return CanMakeMove && !drawable.IsClicked;
    }

    private async Task LeaveGameCommandExecuteAsync()
    {
        if (_isAgainstAi)
        {
            _navigationService.Navigate(Routes.Home);
            return;
        }

        try
        {
            await _signalRClient.LeaveGameAsync().ConfigureAwait(false);
            await _navigationService.NavigateAsync(Routes.PlayersLobby,
                                                   new DynamicDictionary(("FirstPlayerName", FirstPlayerName))).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWithCallerInfo(LogLevel.Error, "Can't leave game due to an error: ", ex);
            _navigationService.Navigate(Routes.Home);
        }
    }

    #endregion

    #region ConnectionEventHandlers

    private void OnConnectionLost()
    {
        DispatcherHelper.InvokeMethodInCorrectThread(() =>
                                                         {
                                                             _ = MessageBox.Show("Соединение с сервером потеряно.\nТекущая сессия будет завершена.", MsgBoxButton.OK, MsgBoxImage.Warning);
                                                         });

        _signalRClient.StopConnectionAsync().SafeFireAndForget();
        _navigationService.Navigate(Routes.Home);
    }

    #endregion
    
    #region GameEventHandlers

    private void OnOpponentMakeMove(int x1, int y1, int x2, int y2, int opponentPlayerScore)
    {
        var line = Lines.First(x => x.X1 == x1 && x.Y1 == y1 && x.X2 == x2 && x.Y2 == y2);
        line.Color = _secondPlayerColor;

        OpponentPlayerScore += opponentPlayerScore;
        CanMakeMove = opponentPlayerScore == 0;
    }

    private void OnGainPoints(int gainPoints)
    {
        CanMakeMove = gainPoints > 0;
        CurrentPlayerScore += gainPoints;
    }

    private async Task OnGameEndAsync()
    {
        await EndGameAsync().ConfigureAwait(false);
    }

    private async Task OnOpponentLeaveGameAsync()
    {
        await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                    {
                                                                        _ = MessageBox.Show("Соперник покинул игру. Текущая сессия будет завершена.", MsgBoxButton.OK, MsgBoxImage.Information);
                                                                    });
        await _navigationService.NavigateAsync(Routes.PlayersLobby, new DynamicDictionary(("FirstPlayerName", FirstPlayerName))).ConfigureAwait(false);
    }

    private async Task EndGameAsync()
    {
        await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                    {
                                                                        _ = MessageBox.Show("Игра окончена!", MsgBoxButton.OK, MsgBoxImage.Information);
                                                                    });

        if (_isAgainstAi)
        {
            await _navigationService.NavigateAsync(Routes.Home).ConfigureAwait(false);
            return;
        }

        await _navigationService.NavigateAsync(Routes.PlayersLobby, new DynamicDictionary(("FirstPlayerName", FirstPlayerName)))
            .ConfigureAwait(false);
    }

    #endregion

    #region AiProcessing

    private async Task ProcessAiLogicAsync(DrawableLine line)
    {
        var gainPoints = _gameController.MakeMove(line.X1, line.Y1, line.Y2);
        CanMakeMove = gainPoints > 0;
        CurrentPlayerScore += gainPoints;

        if (!CanMakeMove)
        {
            await ExecuteAiTurnAsync().ConfigureAwait(false);
        }

        if (_gameController.IsGameEnded())
        {
            await EndGameAsync();
        }
    }

    private async Task ExecuteAiTurnAsync()
    {
        int gainPoints;
        do
        {
            await Task.Delay(2000).ConfigureAwait(false);

            gainPoints = _aiPlayer.MakeMove(Lines);
            OpponentPlayerScore += gainPoints;
    
        } while (gainPoints > 0 && !_gameController.IsGameEnded());
    
        CanMakeMove = true;
    }

    #endregion

    public void Dispose()
    {
        if (_isAgainstAi)
        {
            return;
        }

        _signalRClient.OnOpponentMakeMove -= OnOpponentMakeMove;
        _signalRClient.OnGainPoints += OnGainPoints;
        _signalRClient.OnGameEnd += OnGameEndAsync;
        _signalRClient.OnOpponentLeaveGame -= OnOpponentLeaveGameAsync;
        _signalRClient.OnConnectionLost -= OnConnectionLost;
    }

    #endregion
}