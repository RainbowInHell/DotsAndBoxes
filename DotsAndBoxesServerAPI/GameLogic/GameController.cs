namespace DotsAndBoxesServerAPI;

/// <summary>
/// The GameController class is responsible for managing the game state of the "Dots and Boxes" game.
/// It initializes the game grid, handles line clicks, checks for completed squares, and tracks game progress.
/// </summary>
public class GameController
{
    private readonly int _n; // Represents the number of squares per row and per column (a 4x4 grid, meaning there are 4 squares in each direction).
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

    /// <summary>
    /// Constructor that sets up the game board. It initializes the points and lines, and prepares the arrays that track line clicks.
    /// </summary>
    public GameController(GridSize gridSize)
    {
        _n = GridSizeTypeToInt(gridSize);
        // Calculate the distance between each point on the board.
        // This is done by dividing the total width of the board by the number of squares.
        // For example, if `n` is 4, and the width is 400 pixels, then each point will be 100 pixels apart.
        _distanceBetweenPoints = DefaultWidthHeight / _n;

        // Initialize the boolean arrays that track clicked lines:
        // `linesX` will have `n + 1` rows because there are `n + 1` horizontal lines (1 more than the number of squares).
        // `linesY` will have `n + 1` columns because there are `n + 1` vertical lines (1 more than the number of squares).
        _linesX = new bool[_n + 1, _n];
        _linesY = new bool[_n, _n + 1];
    }

    /// <summary>
    /// Handles what happens when a line is clicked, checking if it results in a completed square.
    /// This method updates the game state and is used by real players and by the AI once a move is confirmed.
    /// </summary>
    public int MakeMove(int startPointX, int startPointY, int endPointX, int endPointY)
    {
        // Update the arrays (`linesX` or `linesY`) to mark this line as clicked for real player moves or confirmed AI moves.
        UpdateLineArrays(startPointX, startPointY, endPointX, endPointY);
        var completedSquaresCounter = 0;

        // Determine the grid coordinates of the line's start point.
        var x = startPointX / _distanceBetweenPoints;
        var y = startPointY / _distanceBetweenPoints;

        // Check if the line is horizontal or vertical, and then determine which squares it could potentially complete:
        // Horizontal lines can complete a square above or below them.
        if (IsHorizontalLine(startPointY, endPointY))
        {
            if (y > 0 && IsSquareComplete(x, y - 1)) completedSquaresCounter++; // Check the square above.
            if (y < _n && IsSquareComplete(x, y)) completedSquaresCounter++; // Check the square below.
        }
        else
        {
            // Vertical lines can complete a square to the left or right of them.
            if (x > 0 && IsSquareComplete(x - 1, y)) completedSquaresCounter++; // Check the square to the left.
            if (x < _n && IsSquareComplete(x, y)) completedSquaresCounter++; // Check the square to the right.
        }

        _completedBoxesTracker += completedSquaresCounter;
        return completedSquaresCounter;
    }

    /// <summary>
    /// Determines if the game has ended by checking if all possible squares are completed.
    /// </summary>
    public bool IsGameEnded() => _completedBoxesTracker == _n * _n;

    /// <summary>
    /// Marks a line as clicked in the appropriate boolean array.
    /// </summary>
    private void UpdateLineArrays(int startPointX, int startPointY, int endPointX, int endPointY)
    {
        // Determine if the line is horizontal or vertical and update the correct array.
        if (IsHorizontalLine(startPointY, endPointY))
        {
            var x = startPointX / _distanceBetweenPoints;
            var y = startPointY / _distanceBetweenPoints;
            _linesX[y, x] = true; // Mark the horizontal line at (x, y) as clicked.
        }
        else
        {
            var x = startPointX / _distanceBetweenPoints;
            var y = startPointY / _distanceBetweenPoints;
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
    private static bool IsHorizontalLine(int startPointY, int endPointY)
    {
        // A line is considered horizontal if its start and end points share the same Y coordinate.
        return startPointY == endPointY;
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