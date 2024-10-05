using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DotsAndBoxesUIComponents;

public class EnumToVisibilityConverter : IValueConverter
{
    private Visibility VisibilityOnActive => Visibility.Collapsed;

    private Visibility VisibilityOnInactive => Visibility.Visible;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var parameterString = parameter?.ToString();
        if (value == null || parameterString == null)
        {
            return parameter == value ? VisibilityOnInactive : VisibilityOnActive;
        }

        if (!Enum.IsDefined(value.GetType(), value))
        {
            return DependencyProperty.UnsetValue;
        }

        var parameterValue = Enum.Parse(value.GetType(), parameterString);
        return parameterValue.Equals(value) ? VisibilityOnInactive : VisibilityOnActive;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}