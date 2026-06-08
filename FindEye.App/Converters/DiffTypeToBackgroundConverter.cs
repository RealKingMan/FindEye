using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using FindEye.Core.Models;

namespace FindEye.App.Converters;

[ValueConversion(typeof(DiffType), typeof(SolidColorBrush))]
public class DiffTypeToBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DiffType dt)
        {
            return dt switch
            {
                DiffType.OnlyInA => new SolidColorBrush(Color.FromRgb(0xE5, 0x3E, 0x3E)),
                DiffType.OnlyInB => new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60)),
                DiffType.ContentDifferent => new SolidColorBrush(Color.FromRgb(0xF3, 0x9C, 0x12)),
                _ => new SolidColorBrush(Color.FromRgb(0x95, 0xA5, 0xA6))
            };
        }
        return Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
