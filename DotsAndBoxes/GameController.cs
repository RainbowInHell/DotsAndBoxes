using System.Drawing;
using System.Windows.Media;
using DotsAndBoxesUIComponents;

namespace DotsAndBoxes;

public class GameController : StateChecker
{
    private const int EllipseSize = 10;

    private const int DefaultWidthHeight = 420;

    private int NumberOfRows { get; }

    private int NumberOfColumns { get; }

    public IReadOnlyList<Point> PointList { get; }

    public IReadOnlyList<LineStructure> LineList { get; }

    public GameController()
    {
        GameHeight = GameWidth = DefaultWidthHeight / 4;
        NumberOfColumns = NumberOfRows = 4;

        PointList = CreatePointList();
        LineList = CreateLineList();
    }

    private IReadOnlyList<Point> CreatePointList()
    {
        var pointList = new List<Point>();

        for (var i = 0; i <= NumberOfRows; ++i)
        {
            for (var j = 0; j <= NumberOfColumns; ++j)
            {
                pointList.Add(CreatePoint(j, i));
            }
        }

        return pointList;
    }

    private IReadOnlyList<LineStructure> CreateLineList()
    {
        var lineList = new List<LineStructure>();

        for (var i = 0; i <= NumberOfRows; ++i)
        {
            for (var j = 0; j < NumberOfColumns; ++j)
            {
                lineList.Add(CreateHorizontalLine(j, i));
            }
        }

        for (var i = 0; i < NumberOfRows; ++i)
        {
            for (var j = 0; j <= NumberOfColumns; ++j)
            {
                lineList.Add(CreateVerticalLine(j, i));
            }
        }

        return lineList;
    }

    private LineStructure CreateHorizontalLine(int positionX, int positionY)
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
            Color = Brushes.Transparent
        };

        return line;
    }

    private LineStructure CreateVerticalLine(int positionX, int positionY)
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
            Color = Brushes.Transparent
        };

        return line;
    }

    private Point CreatePoint(int positionX, int positionY)
    {
        var x = positionX * GameWidth - EllipseSize / 2;
        var y = positionY * GameHeight - EllipseSize / 2;

        return new Point(x, y);
    }
}