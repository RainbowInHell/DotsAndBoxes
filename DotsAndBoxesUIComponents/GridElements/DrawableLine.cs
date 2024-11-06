using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DotsAndBoxesUIComponents;

public partial class DrawableLine : ObservableObject
{
    public bool IsClicked { get; private set; }

    public int X1 { get; init; }

    public int Y1 { get; init; }

    public int X2 { get; init; }

    public int Y2 { get; init; }

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