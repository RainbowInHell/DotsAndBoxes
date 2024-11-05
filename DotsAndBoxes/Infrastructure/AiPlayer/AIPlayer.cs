using System.Windows.Media;
using DotsAndBoxesServerAPI;
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

    public int MakeMove(IReadOnlyCollection<DrawableLine> lines)
    {
        var lineToClick = FindRandomAvailableMove(lines);
        if (lineToClick == null)
        {
            return 0;
        }

        lineToClick.Color = _aiColor;
        return _gameController.MakeMove(lineToClick.StartPoint.X, lineToClick.StartPoint.Y, lineToClick.EndPoint.X, lineToClick.EndPoint.Y);
    }

    private DrawableLine FindRandomAvailableMove(IReadOnlyCollection<DrawableLine> lines)
    {
        var availableLines = lines.Where(line => !line.IsClicked).ToList();
        return availableLines.Count != 0
                   ? availableLines[_random.Next(availableLines.Count)]
                   : null;
    }
}