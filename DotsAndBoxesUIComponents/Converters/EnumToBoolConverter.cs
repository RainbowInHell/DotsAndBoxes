using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DotsAndBoxesUIComponents;

public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var parameterString = parameter?.ToString();
        if (parameterString == null)
        {
            return DependencyProperty.UnsetValue;
        }

        if (value == null || !Enum.IsDefined(value.GetType(), value))
        {
            return DependencyProperty.UnsetValue;
        }

        var parameterValue = Enum.Parse(value.GetType(), parameterString);

        return parameterValue.Equals(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
