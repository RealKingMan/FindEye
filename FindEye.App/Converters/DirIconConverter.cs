using System.Globalization;
using System.Windows.Data;

namespace FindEye.App.Converters;

[ValueConversion(typeof(bool), typeof(string))]
public class DirIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isDir && isDir)
            return "Folder";
        return "FileOutline";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
