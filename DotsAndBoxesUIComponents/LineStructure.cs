using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DotsAndBoxesUIComponents;

public partial class LineStructure : ObservableObject
{
    public int X1 { get; init; }

    public int X2 { get; init; }

    public int Y1 { get; init; }

    public int Y2 { get; init; }

    [ObservableProperty]
    private Brush _color;

    public override bool Equals(object other)
    {
        if (other is not LineStructure toCompareWith)
        {
            return false;
        }

        return this == toCompareWith;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X1, X2, Y1, Y2);
    }

    public static bool operator ==(LineStructure a, LineStructure b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.X1 == b.X1 &&
               a.X2 == b.X2 && a.Y1 == b.Y1 && a.Y2 == b.Y2 ||
               a.X1 == b.X2 &&
               a.X2 == b.X1 && a.Y1 == b.Y2 && a.Y2 == b.Y1;
    }

    public static bool operator !=(LineStructure a, LineStructure b)
    {
        return !(a == b);
    }
}