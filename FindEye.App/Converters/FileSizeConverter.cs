using System.Globalization;
using System.Windows.Data;

namespace FindEye.App.Converters;

[ValueConversion(typeof(long), typeof(string))]
public class FileSizeConverter : IValueConverter
{
    private static readonly string[] Units = { "B", "KB", "MB", "GB", "TB" };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            if (bytes == 0) return "0 B";

            int unitIndex = 0;
            double size = bytes;
            while (size >= 1024 && unitIndex < Units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }
            return $"{size:F1} {Units[unitIndex]}";
        }
        return "0 B";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
