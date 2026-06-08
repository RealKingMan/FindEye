using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FindEye.App.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool boolVal = value is true;
        bool invert = parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase);
        return (boolVal ^ invert) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
