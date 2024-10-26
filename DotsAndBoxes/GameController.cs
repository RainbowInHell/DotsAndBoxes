using DotsAndBoxesUIComponents;

namespace DotsAndBoxes;

public class GameController
{
    const int n = 1;

    private int _completedBoxesTracker;

    private const int DefaultEllipseSize = 10;

    private const int DefaultWidthHeight = 400;

    private readonly int _distanceBetweenPoints;

    private readonly SquareCompletionChecker _squareCompletionChecker;

    public IReadOnlyList<DrawablePoint> PointList { get; }

    public IReadOnlyList<DrawableLine> LineList { get; }

    public GameController()
    {
        _distanceBetweenPoints = DefaultWidthHeight / n;

        PointList = CreatePointList(n, n);
        LineList = CreateLineList(n, n);

        _squareCompletionChecker = new SquareCompletionChecker(LineList, n, _distanceBetweenPoints);
    }

    private List<DrawablePoint> CreatePointList(int numberOfRows, int numberOfColumns)
    {
        var pointList = new List<DrawablePoint>();

        for (var i = 0; i <= numberOfRows; ++i)
        {
            for (var j = 0; j <= numberOfColumns; ++j)
            {
                pointList.Add(CreatePoint(j, i));
            }
        }

        return pointList;
    }

    private List<DrawableLine> CreateLineList(int numberOfRows, int numberOfColumns)
    {
        var lineList = new List<DrawableLine>();

        for (var i = 0; i <= numberOfRows; ++i)
        {
            for (var j = 0; j < numberOfColumns; ++j)
            {
                lineList.Add(CreateHorizontalLine(j, i));
            }
        }

        for (var i = 0; i < numberOfRows; ++i)
        {
            for (var j = 0; j <= numberOfColumns; ++j)
            {
                lineList.Add(CreateVerticalLine(j, i));
            }
        }

        return lineList;
    }

    private DrawableLine CreateHorizontalLine(int positionX, int positionY)
    {
        var x1 = positionX * _distanceBetweenPoints;
        var y1 = positionY * _distanceBetweenPoints;

        var x2 = x1 + _distanceBetweenPoints;
        var y2 = y1;

        var line = new DrawableLine
        {
            StartPoint = new DrawablePoint { X = x1, Y = y1 },
            EndPoint = new DrawablePoint { X = x2, Y = y2 }
        };

        Console.WriteLine($"Created horizontal line ({x1}, {y1}) -> ({x2}, {y2})");
        return line;
    }

    private DrawableLine CreateVerticalLine(int positionX, int positionY)
    {
        var x1 = positionX * _distanceBetweenPoints;
        var y1 = positionY * _distanceBetweenPoints;

        var x2 = x1;
        var y2 = y1 + _distanceBetweenPoints;

        var line = new DrawableLine
        {
            StartPoint = new DrawablePoint { X = x1, Y = y1 },
            EndPoint = new DrawablePoint { X = x2, Y = y2 }
        };

        Console.WriteLine($"Created vertical line ({x1}, {y1}) -> ({x2}, {y2})");
        return line;
    }

    private DrawablePoint CreatePoint(int positionX, int positionY)
    {
        var x = positionX * _distanceBetweenPoints - DefaultEllipseSize / 2;
        var y = positionY * _distanceBetweenPoints - DefaultEllipseSize / 2;

        return new DrawablePoint { X = x, Y = y };
    }

    public bool IsSquareCompleted(DrawableLine drawable)
    {
        var isSquareCompleted = _squareCompletionChecker.IsSquareCompleted(drawable);
        if (isSquareCompleted)
        {
            _completedBoxesTracker++;
            return true;
        }

        return false;
    }

    public bool IsGameEnded() => _completedBoxesTracker == n * n;
}