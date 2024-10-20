using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using DotsAndBoxes.SignalR;
using DotsAndBoxesUIComponents;

namespace DotsAndBoxes.ViewModels;

[Route(Routes.Game)]
public class GameViewModel : BaseViewModel
{
    private readonly SignalRClient _signalRClient;

    private GameController _gameController;

    private readonly Brush _playerColor = Brushes.Red;

    private bool _canMakeMove = true;

    public GameViewModel(SignalRClient signalRClient)
    {
        _signalRClient = signalRClient;
        _gameController = new GameController();

        Lines = new(_gameController.LineList);
        Dots = new(_gameController.PointList);

        ClickLineCommand = new AsyncRelayCommand<LineStructure>(ClickLineCommandExecuteAsync, ClickLineCommandCanExecute);
    }

    #region Properties

    public ObservableCollection<LineStructure> Lines { get; }

    public ObservableCollection<Point> Dots { get; }

    public string FirstPlayer { get; private set; }

    public string SecondPlayer { get; private set; }

    #endregion

    #region Commands

    public ICommand ClickLineCommand { get; }

    #endregion

    #region Methods

    private async Task ClickLineCommandExecuteAsync(LineStructure line)
    {
        line.Color = _playerColor;
    }

    private bool ClickLineCommandCanExecute(LineStructure line)
    {
        return _canMakeMove && line.Color != _playerColor;
    }

    public override NavigationResult OnNavigatedTo(NavigationArgs args)
    {
        // FirstPlayer = args.Parameters.GetValue<string>(nameof(FirstPlayer));
        // SecondPlayer = args.Parameters.GetValue<string>(nameof(SecondPlayer));

        return base.OnNavigatedTo(args);
    }

    #endregion
}