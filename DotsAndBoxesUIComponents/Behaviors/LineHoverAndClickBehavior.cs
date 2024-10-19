using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Xaml.Behaviors;

namespace DotsAndBoxesUIComponents.Behaviors;

public class LineHoverAndClickBehavior : Behavior<Line>
{
    private bool _isClicked;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseEnter += OnMouseEnter;
        AssociatedObject.MouseLeave += OnMouseLeave;
        AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseEnter -= OnMouseEnter;
        AssociatedObject.MouseLeave -= OnMouseLeave;
        AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (!_isClicked && AssociatedObject.DataContext is LineStructure lineStructure)
        {
            lineStructure.Color = Brushes.Black;
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (AssociatedObject.DataContext is LineStructure lineStructure && !_isClicked)
        {
            lineStructure.Color = Brushes.White;
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (AssociatedObject.DataContext is LineStructure)
        {
            _isClicked = true;
        }
    }
}
