using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
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

        _signalRClient.OnPlayerMakeMove += OnPlayerMakeMove;

        Lines = new(_gameController.LineList);
        Dots = new(_gameController.PointList);

        ClickLineCommand = new AsyncRelayCommand<DrawableLine>(ClickLineCommandExecuteAsync, ClickLineCommandCanExecute);
    }

    private async Task EndGameAsync()
    {
        await DispatcherHelper.InvokeMethodInCorrectThreadAsync(() =>
                                                                    {
                                                                        MessageBox.Show("Игра окончена!", MsgBoxButton.OK, MsgBoxImage.Information);
                                                                    });

        await _signalRClient.EndGameAsync().ConfigureAwait(false);
        await _navigationService.NavigateAsync(Routes.PlayersLobby,
                                               new DynamicDictionary(("FirstPlayerName", FirstPlayerName))).ConfigureAwait(false);
    }

    private async Task OnPlayerMakeMove(int x1, int y1, int x2, int y2)
    {
        var line = Lines.First(x => x.StartPoint.X == x1 && x.StartPoint.Y == y1 && x.EndPoint.X == x2 && x.EndPoint.Y == y2);
        line.Color = _secondPlayerColor;

        if(_gameController.IsSquareCompleted(line))
        {
            CanMakeMove = false;
            ++OpponentPlayerScore;

            if (_gameController.IsGameEnded())
            {
                await EndGameAsync().ConfigureAwait(false);
            }
        }
        else
        {
            CanMakeMove = true;
        }
    }

    #region Properties

    public ObservableCollection<DrawableLine> Lines { get; }

    public ObservableCollection<DrawablePoint> Dots { get; }

    public string FirstPlayerName { get; private set; }

    public string SecondPlayerName { get; private set; }

    #endregion

    #region Commands

    public ICommand ClickLineCommand { get; }

    #endregion

    #region Methods

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
            await EndGameAsync().ConfigureAwait(false);
        }
    }

    private bool ClickLineCommandCanExecute(DrawableLine drawable)
    {
        return CanMakeMove && drawable.Color != _secondPlayerColor;
    }

    public override NavigationResult OnNavigatedTo(NavigationArgs args)
    {
        FirstPlayerName = args.Parameters.GetValue<string>(nameof(FirstPlayerName));
        SecondPlayerName = args.Parameters.GetValue<string>(nameof(SecondPlayerName));
        CanMakeMove = args.Parameters.GetValue<bool>("CanMakeMove");

        return base.OnNavigatedTo(args);
    }

    public void Dispose()
    {
        _signalRClient.OnPlayerMakeMove -= OnPlayerMakeMove;
    }

    #endregion
}