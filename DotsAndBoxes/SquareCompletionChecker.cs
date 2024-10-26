using DotsAndBoxesUIComponents;

namespace DotsAndBoxes;

public class SquareCompletionChecker
{
    private readonly IReadOnlyList<DrawableLine> _lineList;

    private readonly int _distanceBetweenPoints;

    private readonly int _n;

    public SquareCompletionChecker(IReadOnlyList<DrawableLine> lineList, int n, int distanceBetweenPoints)
    {
        _lineList = lineList;
        _n = n;
        _distanceBetweenPoints = distanceBetweenPoints;
    }

    public bool IsSquareCompleted(DrawableLine drawable)
    {
        return IsHorizontalLine(drawable)
                   ? CheckHorizontalSquareCompletion(drawable, IsTopLine(drawable), IsBottomLine(drawable))
                   : CheckVerticalSquareCompletion(drawable, IsLeftLine(drawable), IsRightLine(drawable));
    }

    private bool CheckHorizontalSquareCompletion(DrawableLine drawable, bool isTopLine, bool isBottomLine)
    {
        if (isTopLine)
        {
            return AreLinesColored(
                GetLeftLineForTopHorizontalLine(drawable),
                GetRightLineForTopHorizontalLine(drawable),
                GetBottomLineForTopHorizontalLine(drawable)
            );
        }

        if (isBottomLine)
        {
            return AreLinesColored(
                GetLeftLineForBottomHorizontalLine(drawable),
                GetRightLineForBottomHorizontalLine(drawable),
                GetTopLineForBottomHorizontalLine(drawable)
            );
        }

        // MiddleLine
        var topSquare = AreLinesColored(
            GetLeftLineForBottomHorizontalLine(drawable),
            GetRightLineForBottomHorizontalLine(drawable),
            GetTopLineForBottomHorizontalLine(drawable)
        );

        var bottomSquare = AreLinesColored(
            GetLeftLineForTopHorizontalLine(drawable),
            GetRightLineForTopHorizontalLine(drawable),
            GetBottomLineForTopHorizontalLine(drawable)
        );

        return topSquare || bottomSquare;
    }

    private bool CheckVerticalSquareCompletion(DrawableLine drawable, bool isLeftLine, bool isRightLine)
    {
        if (isLeftLine)
        {
            return AreLinesColored(
                GetTopLineForLeftVerticalLine(drawable),
                GetRightLineForLeftVerticalLine(drawable),
                GetBottomLineForLeftVerticalLine(drawable)
            );
        }

        if (isRightLine)
        {
            return AreLinesColored(
                GetTopLineForRightVerticalLine(drawable),
                GetLeftLineForRightVerticalLine(drawable),
                GetBottomLineForRightVerticalLine(drawable)
            );
        }

        // MiddleLine
        var leftSquare = AreLinesColored(
            GetTopLineForRightVerticalLine(drawable),
            GetLeftLineForRightVerticalLine(drawable),
            GetBottomLineForRightVerticalLine(drawable)
        );

        var rightSquare = AreLinesColored(
            GetTopLineForLeftVerticalLine(drawable),
            GetRightLineForLeftVerticalLine(drawable),
            GetBottomLineForLeftVerticalLine(drawable)
        );

        return leftSquare || rightSquare;
    }

    private static bool AreLinesColored(params DrawableLine[] lines)
    {
        return lines.All(line => line.IsClicked);
    }

    private DrawableLine GetBottomLineForTopHorizontalLine(DrawableLine horizontalDrawable)
    {
        return _lineList.First(bottomLine => bottomLine.StartPoint.X == horizontalDrawable.StartPoint.X &&
                                             bottomLine.StartPoint.Y == horizontalDrawable.StartPoint.Y + _distanceBetweenPoints &&
                                             bottomLine.EndPoint.X == horizontalDrawable.EndPoint.X &&
                                             bottomLine.EndPoint.Y == horizontalDrawable.EndPoint.Y + _distanceBetweenPoints);
    }

    private DrawableLine GetRightLineForTopHorizontalLine(DrawableLine horizontalDrawable)
    {
        return _lineList.First(rightLine => rightLine.StartPoint.X == horizontalDrawable.EndPoint.X &&
                                            rightLine.StartPoint.Y == horizontalDrawable.EndPoint.Y &&
                                            rightLine.EndPoint.X == horizontalDrawable.EndPoint.X &&
                                            rightLine.EndPoint.Y == horizontalDrawable.EndPoint.Y + _distanceBetweenPoints);
    }

    private DrawableLine GetLeftLineForTopHorizontalLine(DrawableLine horizontalDrawable)
    {
        return _lineList.First(leftLine => leftLine.StartPoint.X == horizontalDrawable.StartPoint.X &&
                                           leftLine.StartPoint.Y == horizontalDrawable.StartPoint.Y &&
                                           leftLine.EndPoint.X == horizontalDrawable.StartPoint.X &&
                                           leftLine.EndPoint.Y == horizontalDrawable.StartPoint.Y + _distanceBetweenPoints);
    }

    private DrawableLine GetTopLineForBottomHorizontalLine(DrawableLine horizontalDrawable)
    {
        return _lineList.First(topLine => topLine.StartPoint.X == horizontalDrawable.StartPoint.X &&
                                          topLine.StartPoint.Y == horizontalDrawable.StartPoint.Y - _distanceBetweenPoints &&
                                          topLine.EndPoint.X == horizontalDrawable.EndPoint.X &&
                                          topLine.EndPoint.Y == horizontalDrawable.EndPoint.Y - _distanceBetweenPoints);
    }

    private DrawableLine GetRightLineForBottomHorizontalLine(DrawableLine horizontalDrawable)
    {
        return _lineList.First(rightLine => rightLine.StartPoint.X == horizontalDrawable.EndPoint.X &&
                                            rightLine.StartPoint.Y == horizontalDrawable.EndPoint.Y - _distanceBetweenPoints &&
                                            rightLine.EndPoint.X == horizontalDrawable.EndPoint.X &&
                                            rightLine.EndPoint.Y == horizontalDrawable.EndPoint.Y);
    }

    private DrawableLine GetLeftLineForBottomHorizontalLine(DrawableLine horizontalDrawable)
    {
        return _lineList.First(leftLine => leftLine.StartPoint.X == horizontalDrawable.StartPoint.X &&
                                           leftLine.StartPoint.Y == horizontalDrawable.StartPoint.Y - _distanceBetweenPoints &&
                                           leftLine.EndPoint.X == horizontalDrawable.StartPoint.X &&
                                           leftLine.EndPoint.Y == horizontalDrawable.StartPoint.Y);
    }

    private DrawableLine GetTopLineForLeftVerticalLine(DrawableLine verticalDrawable)
    {
        return _lineList.First(topLine => topLine.StartPoint.X == verticalDrawable.StartPoint.X &&
                                          topLine.StartPoint.Y == verticalDrawable.StartPoint.Y &&
                                          topLine.EndPoint.X == verticalDrawable.StartPoint.X + _distanceBetweenPoints &&
                                          topLine.EndPoint.Y == verticalDrawable.StartPoint.Y);
    }

    private DrawableLine GetBottomLineForLeftVerticalLine(DrawableLine verticalDrawable)
    {
        return _lineList.First(bottomLine => bottomLine.StartPoint.X == verticalDrawable.EndPoint.X &&
                                             bottomLine.StartPoint.Y == verticalDrawable.EndPoint.Y &&
                                             bottomLine.EndPoint.X == verticalDrawable.EndPoint.X + _distanceBetweenPoints &&
                                             bottomLine.EndPoint.Y == verticalDrawable.EndPoint.Y);
    }

    private DrawableLine GetRightLineForLeftVerticalLine(DrawableLine verticalDrawable)
    {
        return _lineList.First(rightLine => rightLine.StartPoint.X == verticalDrawable.StartPoint.X + _distanceBetweenPoints &&
                                            rightLine.StartPoint.Y == verticalDrawable.StartPoint.Y &&
                                            rightLine.EndPoint.X == verticalDrawable.EndPoint.X + _distanceBetweenPoints &&
                                            rightLine.EndPoint.Y == verticalDrawable.EndPoint.Y);
    }

    private DrawableLine GetTopLineForRightVerticalLine(DrawableLine verticalDrawable)
    {
        return _lineList.First(topLine => topLine.StartPoint.X == verticalDrawable.StartPoint.X - _distanceBetweenPoints &&
                                          topLine.StartPoint.Y == verticalDrawable.StartPoint.Y &&
                                          topLine.EndPoint.X == verticalDrawable.StartPoint.X &&
                                          topLine.EndPoint.Y == verticalDrawable.StartPoint.Y);
    }

    private DrawableLine GetBottomLineForRightVerticalLine(DrawableLine verticalDrawable)
    {
        return _lineList.First(bottomLine => bottomLine.StartPoint.X == verticalDrawable.EndPoint.X - _distanceBetweenPoints &&
                                             bottomLine.StartPoint.Y == verticalDrawable.EndPoint.Y &&
                                             bottomLine.EndPoint.X == verticalDrawable.EndPoint.X &&
                                             bottomLine.EndPoint.Y == verticalDrawable.EndPoint.Y);
    }

    private DrawableLine GetLeftLineForRightVerticalLine(DrawableLine verticalDrawable)
    {
        return _lineList.First(leftLine => leftLine.StartPoint.X == verticalDrawable.StartPoint.X - _distanceBetweenPoints &&
                                           leftLine.StartPoint.Y == verticalDrawable.StartPoint.Y &&
                                           leftLine.EndPoint.X == verticalDrawable.EndPoint.X - _distanceBetweenPoints &&
                                           leftLine.EndPoint.Y == verticalDrawable.EndPoint.Y);
    }

    private static bool IsHorizontalLine(DrawableLine drawable) => drawable.StartPoint.Y == drawable.EndPoint.Y;

    private static bool IsTopLine(DrawableLine drawable) => drawable.StartPoint.Y - 1 < 0;

    private bool IsBottomLine(DrawableLine drawable) => drawable.StartPoint.Y == _distanceBetweenPoints * _n;

    private static bool IsLeftLine(DrawableLine drawable) => drawable.StartPoint.X - 1 < 0;

    private bool IsRightLine(DrawableLine drawable) => drawable.StartPoint.X == _distanceBetweenPoints * _n;
}
