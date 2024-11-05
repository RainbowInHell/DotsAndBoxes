using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using DotsAndBoxesServerAPI;

namespace DotsAndBoxesUIComponents;

public class EnumToDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is Enum)
        {
            parameter = value.GetType();
        }

        return parameter is not Type ? string.Empty : GetLocalizedDescription(value as Enum);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string GetLocalizedDescription(Enum value)
    {
        return GetEnumAttribute<ExtendedDisplayNameAttribute>(value)?.DisplayName ?? value.ToString();
    }

    private static T GetEnumAttribute<T>(Enum value) where T : Attribute
    { 
        var attributes = GetEnumAttributes<T>(value);
        return attributes.FirstOrDefault();
    }

    private static List<T> GetEnumAttributes<T>(Enum value) where T : Attribute
    {
        var attributes = value.GetType()
            .GetField(value.ToString())?
            .GetCustomAttributes<T>(false)
            .ToList();

        return attributes is { Count: not 0 } ? attributes : [];
    }
}