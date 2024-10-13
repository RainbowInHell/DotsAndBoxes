using System.Globalization;
using System.Windows.Data;

namespace DotsAndBoxesUIComponents;

public class MultiBoolToBoolConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValues = values.OfType<bool>().ToList();

        if (parameter is string paramAsString)
        {
            if (paramAsString.Equals("AND", StringComparison.OrdinalIgnoreCase))
            {
                return boolValues.Aggregate(true, (current, boolValue) => current && boolValue);
            }

            if (paramAsString.Equals("OR", StringComparison.OrdinalIgnoreCase))
            {
                return boolValues.Aggregate(false, (current, boolValue) => current || boolValue);
            }
        }

        return boolValues.Aggregate(true, (current, boolValue) => current && boolValue);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [value];
    }
}
