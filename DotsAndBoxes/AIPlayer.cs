using System.Windows.Media;
using DotsAndBoxesUIComponents;

namespace DotsAndBoxes;

public class AiPlayer
{
    private readonly GameController _gameController;
    private readonly Brush _aiColor;
    private readonly Random _random;

    public AiPlayer(GameController gameController, Brush aiColor)
    {
        _gameController = gameController;
        _aiColor = aiColor;
        _random = new Random();
    }

    /// <summary>
    /// Makes a move for the AI by selecting a random unclicked line and returns the points gained (0 or 1).
    /// </summary>
    public int MakeMove(IReadOnlyCollection<DrawableLine> lines)
    {
        var lineToClick = FindRandomAvailableMove(lines);
        if (lineToClick == null)
        {
            return 0;
        }

        lineToClick.Color = _aiColor;
        return _gameController.MakeMove(lineToClick);
    }

    /// <summary>
    /// Finds a random unclicked line for the AI to select as its move.
    /// </summary>
    private DrawableLine FindRandomAvailableMove(IReadOnlyCollection<DrawableLine> lines)
    {
        // Get a list of unclicked lines.
        var availableLines = lines.Where(line => !line.IsClicked).ToList();

        // Select a random unclicked line if available.
        return availableLines.Count != 0 ? availableLines[_random.Next(availableLines.Count)] : null;
    }
}
