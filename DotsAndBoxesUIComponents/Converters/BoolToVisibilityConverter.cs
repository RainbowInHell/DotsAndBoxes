using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DotsAndBoxesUIComponents;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool state)
        {
            return Visibility.Collapsed;
        }

        if (parameter is string parameterString && parameterString.Equals("Invert", StringComparison.OrdinalIgnoreCase))
        {
            state = !state;
        }

        return state ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}