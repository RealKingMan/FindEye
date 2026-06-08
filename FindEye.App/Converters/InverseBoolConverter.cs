using System.Globalization;
using System.Windows.Data;

namespace FindEye.App.Converters;

[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? false : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? false : true;
}
