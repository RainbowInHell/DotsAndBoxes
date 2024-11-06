namespace DotsAndBoxesServerAPI;

public class GameController
{
    /// <summary>
    /// The total width and height of the game board in pixels.
    /// </summary>
    private const int DefaultWidthHeight = 400;

    /// <summary>
    /// Represents the number of squares per row and per column (a 4x4 grid, meaning there are 4 squares in each direction).
    /// </summary>
    private readonly int _n;

    /// <summary>
    /// The distance between each point on the grid, calculated based on the board size and the number of squares.
    /// For example, if the board is 400 pixels wide and there are 4 squares, the distance would be 100 pixels.
    /// </summary>
    private readonly int _distanceBetweenPoints;

    /// <summary>
    /// Tracks horizontal lines (lines that go left-to-right).
    /// </summary>
    private readonly bool[,] _horizontalLines;

    /// <summary>
    /// Tracks vertical lines (lines that go up-and-down).
    /// </summary>
    private readonly bool[,] _verticalLines;

    /// <summary>
    /// This variable keeps track of how many squares have been completed so far.
    /// </summary>
    private int _completedBoxesTracker;

    public GameController(GridSize gridSize)
    {
        _n = GridSizeTypeToInt(gridSize);
        _distanceBetweenPoints = DefaultWidthHeight / _n;

        // `n + 1` rows because there are `n + 1` horizontal lines (1 more than the number of squares).
        _horizontalLines = new bool[_n + 1, _n];

        // `n + 1` columns because there are `n + 1` vertical lines (1 more than the number of squares).
        _verticalLines = new bool[_n, _n + 1];
    }

    public int MakeMove(int x1, int y1, int y2)
    {
        var newlyCompletedSquares = 0;

        // Calculate the grid cell coordinates based on the start point.
        var gridColumnIndex = x1 / _distanceBetweenPoints;
        var gridRowIndex = y1 / _distanceBetweenPoints;

        // Determine if the selected line is horizontal based on Y-coordinates.
        var isHorizontal = IsHorizontalLine(y1, y2);

        // Mark the line as clicked in the appropriate array.
        MarkLineAsClicked(gridColumnIndex, gridRowIndex, isHorizontal);

        // Check adjacent squares to see if the newly clicked line completes any square.
        if (isHorizontal)
        {
            // For horizontal lines, check the square above and the square below.
            if (gridRowIndex > 0 && IsSquareCompleted(gridColumnIndex, gridRowIndex - 1)) newlyCompletedSquares++; // Square above.
            if (gridRowIndex < _n && IsSquareCompleted(gridColumnIndex, gridRowIndex)) newlyCompletedSquares++; // Square below.
        }
        else
        {
            // For vertical lines, check the square to the left and the square to the right.
            if (gridColumnIndex > 0 && IsSquareCompleted(gridColumnIndex - 1, gridRowIndex)) newlyCompletedSquares++; // Square to the left.
            if (gridColumnIndex < _n && IsSquareCompleted(gridColumnIndex, gridRowIndex)) newlyCompletedSquares++; // Square to the right.
        }

        _completedBoxesTracker += newlyCompletedSquares;
        return newlyCompletedSquares;
    }

    public bool IsGameEnded() => _completedBoxesTracker == _n * _n;

    private void MarkLineAsClicked(int gridColumnIndex, int gridRowIndex, bool isHorizontal)
    {
        // Mark the selected line as "clicked" in the corresponding line tracking array.
        if (isHorizontal)
        {
            // For horizontal lines, mark the line in the _horizontalLines array at (gridY, gridX).
            _horizontalLines[gridRowIndex, gridColumnIndex] = true;
        }
        else
        {
            // For vertical lines, mark the line in the _verticalLines array at (gridY, gridX).
            _verticalLines[gridRowIndex, gridColumnIndex] = true;
        }
    }

    private bool IsSquareCompleted(int gridColumnIndex, int gridRowIndex)
    {
        var topBoundaryClicked = _horizontalLines[gridRowIndex, gridColumnIndex];
        var bottomBoundaryClicked = _horizontalLines[gridRowIndex + 1, gridColumnIndex];
        var leftBoundaryClicked = _verticalLines[gridRowIndex, gridColumnIndex];
        var rightBoundaryClicked = _verticalLines[gridRowIndex, gridColumnIndex + 1];

        return topBoundaryClicked && bottomBoundaryClicked && leftBoundaryClicked && rightBoundaryClicked;
    }

    private static bool IsHorizontalLine(int y1, int y2)
    {
        return y1 == y2;
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