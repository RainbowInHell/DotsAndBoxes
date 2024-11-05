using DotsAndBoxesServerAPI;

namespace DotsAndBoxesUIComponents;

public static class BoardDrawer
{
    // A default size used for drawing points (dots) on the UI.
    private const int DefaultEllipseSize = 10;

    // The total width and height of the game board in pixels.
    private const int DefaultWidthHeight = 400;

    // These are read-only lists that hold all the points (dots) and lines on the game board.
    // These lists are used to render (draw) the game board on the screen.
    public static (IReadOnlyCollection<DrawablePoint> pointList, IReadOnlyCollection<DrawableLine> lineList)
        DrawBoard(GridSize gridSize)
    {
        // Represents the number of squares per row and per column (a 4x4 grid, meaning there are 4 squares in each direction).
        var n = GridSizeTypeToInt(gridSize);

        // The distance between each point on the grid, calculated based on the board size and the number of squares.
        // For example, if the board is 400 pixels wide and there are 4 squares, the distance would be 100 pixels.
        var distanceBetweenPoints = DefaultWidthHeight / n;

        return (CreatePointList(n, n, distanceBetweenPoints), CreateLineList(n, n, distanceBetweenPoints));
    }

    /// <summary>
    /// Creates a list of points (vertices) that form the intersections on the game grid.
    /// </summary>
    private static List<DrawablePoint> CreatePointList(int numberOfRows,
                                                                      int numberOfColumns,
                                                                      int distanceBetweenPoints)
    {
        var pointList = new List<DrawablePoint>();

        // Loop through each row and column to generate the points.
        // We use `<=` because there are `n + 1` points along each axis (1 more than the number of squares).
        for (var i = 0; i <= numberOfRows; ++i)
        {
            for (var j = 0; j <= numberOfColumns; ++j)
            {
                // Create a point at the given row (`i`) and column (`j`) and add it to the list.
                pointList.Add(CreatePoint(j, i, distanceBetweenPoints));
            }
        }

        return pointList;
    }

    /// <summary>
    /// Creates a list of horizontal and vertical lines that form the borders of the squares on the game grid.
    /// </summary>
    private static List<DrawableLine> CreateLineList(int numberOfRows,
                                                     int numberOfColumns,
                                                     int distanceBetweenPoints)
    {
        var lineList = new List<DrawableLine>();

        // Generate the horizontal lines (left-to-right lines).
        // For each row (`i`), create lines across `numberOfColumns`.
        for (var i = 0; i <= numberOfRows; ++i)
        {
            for (var j = 0; j < numberOfColumns; ++j)
            {
                lineList.Add(CreateHorizontalLine(j, i, distanceBetweenPoints));
            }
        }

        // Generate the vertical lines (up-and-down lines).
        // For each column (`j`), create lines down `numberOfRows`.
        for (var i = 0; i < numberOfRows; ++i)
        {
            for (var j = 0; j <= numberOfColumns; ++j)
            {
                lineList.Add(CreateVerticalLine(j, i, distanceBetweenPoints));
            }
        }

        return lineList;
    }

    /// <summary>
    /// Creates a horizontal line based on its grid position.
    /// </summary>
    private static DrawableLine CreateHorizontalLine(int positionX,
                                                     int positionY,
                                                     int distanceBetweenPoints)
    {
        // Calculate where the line should start and end.
        // A horizontal line starts at (`x1`, `y1`) and ends at (`x2`, `y2`), where `y1` and `y2` are the same.
        var x1 = positionX * distanceBetweenPoints;
        var y1 = positionY * distanceBetweenPoints;
        var x2 = x1 + distanceBetweenPoints;
        var y2 = y1;

        return new DrawableLine
        {
            StartPoint = new DrawablePoint { X = x1, Y = y1 },
            EndPoint = new DrawablePoint { X = x2, Y = y2 }
        };
    }

    /// <summary>
    /// Creates a vertical line based on its grid position.
    /// </summary>
    private static DrawableLine CreateVerticalLine(int positionX,
                                                   int positionY,
                                                   int distanceBetweenPoints)
    {
        // Calculate where the line should start and end.
        // A vertical line starts at (`x1`, `y1`) and ends at (`x2`, `y2`), where `x1` and `x2` are the same.
        var x1 = positionX * distanceBetweenPoints;
        var y1 = positionY * distanceBetweenPoints;
        var x2 = x1;
        var y2 = y1 + distanceBetweenPoints;

        return new DrawableLine
        {
            StartPoint = new DrawablePoint { X = x1, Y = y1 },
            EndPoint = new DrawablePoint { X = x2, Y = y2 }
        };
    }

    /// <summary>
    /// Creates a point based on its grid position, adjusting for its size on the screen.
    /// </summary>
    private static DrawablePoint CreatePoint(int positionX,
                                             int positionY,
                                             int distanceBetweenPoints)
    {
        // Calculate where the point should be placed on the screen.
        // Adjust the coordinates so that the point appears centered around its grid location.
        var x = positionX * distanceBetweenPoints - DefaultEllipseSize / 2;
        var y = positionY * distanceBetweenPoints - DefaultEllipseSize / 2;
        return new DrawablePoint { X = x, Y = y };
    }

    private static int GridSizeTypeToInt(GridSize gridSize)
    {
        return gridSize switch
        {
            GridSize.ThreeToThree => 3,
            GridSize.FiveToFive => 5,
            GridSize.SixToSix => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(gridSize), gridSize, null)
        };
    }
}