using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DotsAndBoxesUIComponents;

public readonly struct DrawablePoint
{
    public int X { get; init; }

    public int Y { get; init; }
}

public partial class DrawableLine : ObservableObject
{
    public bool IsClicked { get; private set; }

    public DrawablePoint StartPoint { get; init; }

    public DrawablePoint EndPoint { get; init; }

    [ObservableProperty]
    private Brush _color = Brushes.Transparent;

    partial void OnColorChanged(Brush value)
    {
        if (value != Brushes.Black && value != Brushes.White && value != Brushes.Transparent)
        {
            IsClicked = true;
        }
    }
}