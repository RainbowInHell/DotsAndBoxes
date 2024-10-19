using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using DotsAndBoxes.Attributes;
using DotsAndBoxes.Navigation;
using DotsAndBoxesUIComponents;

namespace DotsAndBoxes.ViewModels;

[Route(Routes.Game)]
public class GameViewModel : BaseViewModel
{
    private GameController _gameController;

    private readonly Brush _playerColor = Brushes.Red;

    private bool _canMakeMove = true;

    public GameViewModel()
    {
        _gameController = new GameController();
        _gameController.Initialize(420, 420);

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
        await Task.Delay(1);
    }

    private bool ClickLineCommandCanExecute(LineStructure line)
    {
        return _canMakeMove && line.Color != _playerColor;
    }

    #endregion
}