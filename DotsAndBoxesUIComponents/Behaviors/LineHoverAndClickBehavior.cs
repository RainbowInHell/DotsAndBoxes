using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DotsAndBoxesUIComponents.GridElements;
using Microsoft.Xaml.Behaviors;

namespace DotsAndBoxesUIComponents.Behaviors;

public class LineHoverAndClickBehavior : Behavior<Line>
{
    public static readonly DependencyProperty CanClickProperty =
        DependencyProperty.Register(
            nameof(CanClick), 
            typeof(bool), 
            typeof(LineHoverAndClickBehavior), 
            new PropertyMetadata(true));

    public bool CanClick
    {
        get => (bool)GetValue(CanClickProperty);
        set => SetValue(CanClickProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseEnter += OnMouseEnter;
        AssociatedObject.MouseLeave += OnMouseLeave;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseEnter -= OnMouseEnter;
        AssociatedObject.MouseLeave -= OnMouseLeave;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (AssociatedObject.DataContext is DrawableLine { IsClicked: false } lineStructure)
        {
            lineStructure.Color = Brushes.Black;
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (AssociatedObject.DataContext is DrawableLine { IsClicked: false } lineStructure)
        {
            lineStructure.Color = Brushes.White;
        }
    }
}
