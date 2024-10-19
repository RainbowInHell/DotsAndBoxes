using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;
using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesUIComponents;

namespace DotsAndBoxes;

public class GameController : StateChecker
{
    private GameState _gameState;

    private GridToPlayType _gameType;
    private int _gridSize;

    private List<Point> _pointList;
    private Random _random;

    public int EllipseSize { get; } = 10;

    public int TurnId
    {
        get => _gameState.TurnId;
        private set => _gameState.TurnId = value;
    }

    public ReadOnlyCollection<int> Scores => new ReadOnlyCollection<int>(_gameState.Scores);

    public int NumberOfRows { get; private set; }
    public int NumberOfColums { get; private set; }

    public int TimeElapsed { get; set; }

    public ReadOnlyCollection<Point> PointList => new ReadOnlyCollection<Point>(_pointList);

    public ReadOnlyCollection<LineStructure> LineList => new ReadOnlyCollection<LineStructure>(_gameState.LineList);

    public void Initialize(double canvasHeight, double canvasWidth)
    {
        _pointList = new List<Point>();

        StartNewGame(canvasWidth);
    }

    private void StartNewGame(double canvasWidth)
    {
        CreateNewGameState();

        GameWidth = (int) canvasWidth / 8;
        GameHeight = GameWidth;
        NumberOfRows = 8;
        NumberOfColums = NumberOfRows;

        CreateEllipsePositionList();
        CreateLineList(Brushes.Transparent);
    }

    private void CreateNewGameState()
    {
        _gameState = new GameState
        {
            GridSize = _gridSize,
            TurnId = 1,
            Player1 = "First",
            Player2 = "Second"
        };
    }

    public void CreateEllipsePositionList()
    {
        CreateEllipsePositionListForClassicView();
    }

    public void CreateLineList(Brush brush)
    {
        CreateClassicGrid(brush);
    }

    private void CreateClassicGrid(Brush brush)
    {
        for (var i = 0; i <= NumberOfRows; ++i)
        for (var j = 0; j < NumberOfColums; ++j)
            AddHorizontalLine(j, i, brush);

        for (var i = 0; i < NumberOfRows; ++i)
        for (var j = 0; j <= NumberOfColums; ++j)
            AddVerticalLine(j, i, brush);
    }

    private void AddHorizontalLine(int positionX, int positionY, Brush brush)
    {
        var x1 = positionX * GameWidth;
        var y1 = positionY * GameHeight;
        var x2 = x1 + GameWidth;
        var y2 = y1;
        var line = new LineStructure
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Color = brush,
            // StrokeThickness = 8
        };
        _gameState.LineList.Add(line);
    }

    private void AddVerticalLine(int positionX, int positionY, Brush brush)
    {
        var x1 = positionX * GameWidth;
        var y1 = positionY * GameHeight;
        var x2 = x1;
        var y2 = y1 + GameHeight;
        var line = new LineStructure
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Color = brush,
            // StrokeThickness = 8
        };
        _gameState.LineList.Add(line);
    }

    private void CreateEllipsePositionListForClassicView()
    {
        for (var i = 0; i <= NumberOfRows; ++i)
        for (var j = 0; j <= NumberOfColums; ++j)
            AddPoint(j, i);
    }

    private void AddPoint(int positionX, int positionY)
    {
        var x = positionX * GameWidth - EllipseSize / 2;
        var y = positionY * GameHeight - EllipseSize / 2;

        _pointList.Add(new Point(x, y));
    }
}