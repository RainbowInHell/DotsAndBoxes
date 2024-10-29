using DotsAndBoxesUIComponents;

namespace DotsAndBoxes;

/// <summary>
/// The GameController class is responsible for managing the game state of the "Dots and Boxes" game.
/// It initializes the game grid, handles line clicks, checks for completed squares, and tracks game progress.
/// </summary>
public class GameController
{
    // Constants that define the game settings.
    private const int N = 10; // Represents the number of squares per row and per column (a 4x4 grid, meaning there are 4 squares in each direction).
    private const int DefaultEllipseSize = 10; // A default size used for drawing points (dots) on the UI.
    private const int DefaultWidthHeight = 400; // The total width and height of the game board in pixels.

    // This variable keeps track of how many squares have been completed so far.
    private int _completedBoxesTracker;

    // The distance between each point on the grid, calculated based on the board size and the number of squares.
    // For example, if the board is 400 pixels wide and there are 4 squares, the distance would be 100 pixels.
    private readonly int _distanceBetweenPoints;

    // Two-dimensional arrays that help track which lines have been clicked:
    // - `linesX` tracks horizontal lines (lines that go left-to-right).
    // - `linesY` tracks vertical lines (lines that go up-and-down).
    // Each entry in these arrays is either `true` (line has been clicked) or `false` (line has not been clicked).
    private readonly bool[,] _linesX; // Tracks the horizontal lines.
    private readonly bool[,] _linesY; // Tracks the vertical lines.

    // These are read-only lists that hold all the points (dots) and lines on the game board.
    // These lists are used to render (draw) the game board on the screen.
    public IReadOnlyList<DrawablePoint> PointList { get; }
    public IReadOnlyList<DrawableLine> LineList { get; }

    /// <summary>
    /// Constructor that sets up the game board. It initializes the points and lines, and prepares the arrays that track line clicks.
    /// </summary>
    public GameController()
    {
        // Calculate the distance between each point on the board.
        // This is done by dividing the total width of the board by the number of squares.
        // For example, if `n` is 4, and the width is 400 pixels, then each point will be 100 pixels apart.
        _distanceBetweenPoints = DefaultWidthHeight / N;

        // Initialize the list of points (dots) and lines (horizontal and vertical).
        PointList = CreatePointList(N, N);
        LineList = CreateLineList(N, N);

        // Initialize the boolean arrays that track clicked lines:
        // `linesX` will have `n + 1` rows because there are `n + 1` horizontal lines (1 more than the number of squares).
        // `linesY` will have `n + 1` columns because there are `n + 1` vertical lines (1 more than the number of squares).
        _linesX = new bool[N + 1, N];
        _linesY = new bool[N, N + 1];
    }

    /// <summary>
    /// Creates a list of points (vertices) that form the intersections on the game grid.
    /// </summary>
    private List<DrawablePoint> CreatePointList(int numberOfRows, int numberOfColumns)
    {
        var pointList = new List<DrawablePoint>();

        // Loop through each row and column to generate the points.
        // We use `<=` because there are `n + 1` points along each axis (1 more than the number of squares).
        for (var i = 0; i <= numberOfRows; ++i)
        {
            for (var j = 0; j <= numberOfColumns; ++j)
            {
                // Create a point at the given row (`i`) and column (`j`) and add it to the list.
                pointList.Add(CreatePoint(j, i));
            }
        }

        // Return the completed list of points.
        return pointList;
    }

    /// <summary>
    /// Creates a list of horizontal and vertical lines that form the borders of the squares on the game grid.
    /// </summary>
    private List<DrawableLine> CreateLineList(int numberOfRows, int numberOfColumns)
    {
        var lineList = new List<DrawableLine>();

        // Generate the horizontal lines (left-to-right lines).
        // For each row (`i`), create lines across `numberOfColumns`.
        for (var i = 0; i <= numberOfRows; ++i)
        {
            for (var j = 0; j < numberOfColumns; ++j)
            {
                lineList.Add(CreateHorizontalLine(j, i));
            }
        }

        // Generate the vertical lines (up-and-down lines).
        // For each column (`j`), create lines down `numberOfRows`.
        for (var i = 0; i < numberOfRows; ++i)
        {
            for (var j = 0; j <= numberOfColumns; ++j)
            {
                lineList.Add(CreateVerticalLine(j, i));
            }
        }

        // Return the completed list of lines.
        return lineList;
    }

    /// <summary>
    /// Creates a horizontal line based on its grid position.
    /// </summary>
    private DrawableLine CreateHorizontalLine(int positionX, int positionY)
    {
        // Calculate where the line should start and end.
        // A horizontal line starts at (`x1`, `y1`) and ends at (`x2`, `y2`), where `y1` and `y2` are the same.
        var x1 = positionX * _distanceBetweenPoints;
        var y1 = positionY * _distanceBetweenPoints;
        var x2 = x1 + _distanceBetweenPoints;
        var y2 = y1;

        // Create and return a new line with the calculated start and end points.
        return new DrawableLine
        {
            StartPoint = new DrawablePoint { X = x1, Y = y1 },
            EndPoint = new DrawablePoint { X = x2, Y = y2 }
        };
    }

    /// <summary>
    /// Creates a vertical line based on its grid position.
    /// </summary>
    private DrawableLine CreateVerticalLine(int positionX, int positionY)
    {
        // Calculate where the line should start and end.
        // A vertical line starts at (`x1`, `y1`) and ends at (`x2`, `y2`), where `x1` and `x2` are the same.
        var x1 = positionX * _distanceBetweenPoints;
        var y1 = positionY * _distanceBetweenPoints;
        var x2 = x1;
        var y2 = y1 + _distanceBetweenPoints;

        // Create and return a new line with the calculated start and end points.
        return new DrawableLine
        {
            StartPoint = new DrawablePoint { X = x1, Y = y1 },
            EndPoint = new DrawablePoint { X = x2, Y = y2 }
        };
    }

    /// <summary>
    /// Creates a point based on its grid position, adjusting for its size on the screen.
    /// </summary>
    private DrawablePoint CreatePoint(int positionX, int positionY)
    {
        // Calculate where the point should be placed on the screen.
        // Adjust the coordinates so that the point appears centered around its grid location.
        var x = positionX * _distanceBetweenPoints - DefaultEllipseSize / 2;
        var y = positionY * _distanceBetweenPoints - DefaultEllipseSize / 2;
        return new DrawablePoint { X = x, Y = y };
    }

    /// <summary>
    /// Handles what happens when a line is clicked, checking if it results in a completed square.
    /// </summary>
    public bool IsSquareCompleted(DrawableLine drawable)
    {
        // Update the arrays (`linesX` or `linesY`) to mark this line as clicked.
        UpdateLineArrays(drawable);
        var isCompleted = false;

        // Determine the grid coordinates of the line's start point.
        var x = drawable.StartPoint.X / _distanceBetweenPoints;
        var y = drawable.StartPoint.Y / _distanceBetweenPoints;

        // Check if the line is horizontal or vertical, and then determine which squares it could potentially complete:
        // Horizontal lines can complete a square above or below them.
        if (IsHorizontalLine(drawable))
        {
            if (y > 0 && IsSquareComplete(x, y - 1)) isCompleted = true; // Check the square above.
            if (y < N && IsSquareComplete(x, y)) isCompleted = true; // Check the square below.
        }
        else
        {
            // Vertical lines can complete a square to the left or right of them.
            if (x > 0 && IsSquareComplete(x - 1, y)) isCompleted = true; // Check the square to the left.
            if (x < N && IsSquareComplete(x, y)) isCompleted = true; // Check the square to the right.
        }

        // If a square was completed, increase the completed box counter.
        if (isCompleted) _completedBoxesTracker++;

        // Return whether a square was completed by this action.
        return isCompleted;
    }

    /// <summary>
    /// Determines if the game has ended by checking if all possible squares are completed.
    /// </summary>
    public bool IsGameEnded() => _completedBoxesTracker == N * N;

    /// <summary>
    /// Marks a line as clicked in the appropriate boolean array.
    /// </summary>
    private void UpdateLineArrays(DrawableLine line)
    {
        // Determine if the line is horizontal or vertical and update the correct array.
        if (IsHorizontalLine(line))
        {
            var x = line.StartPoint.X / _distanceBetweenPoints;
            var y = line.StartPoint.Y / _distanceBetweenPoints;
            _linesX[y, x] = true; // Mark the horizontal line at (x, y) as clicked.
        }
        else
        {
            var x = line.StartPoint.X / _distanceBetweenPoints;
            var y = line.StartPoint.Y / _distanceBetweenPoints;
            _linesY[y, x] = true; // Mark the vertical line at (x, y) as clicked.
        }
    }

    /// <summary>
    /// Checks if a square at the specified grid position is complete (all four lines around it are clicked).
    /// </summary>
    private bool IsSquareComplete(int x, int y)
    {
        // Check if all four sides of the square at (x, y) are clicked.
        return _linesX[y, x] && _linesX[y + 1, x] && _linesY[y, x] && _linesY[y, x + 1];
    }

    /// <summary>
    /// Determines if the given line is horizontal by comparing its start and end points.
    /// </summary>
    private static bool IsHorizontalLine(DrawableLine line)
    {
        // A line is considered horizontal if its start and end points share the same Y coordinate.
        return line.StartPoint.Y == line.EndPoint.Y;
    }
}
