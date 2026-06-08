using System.Globalization;
using System.Windows.Data;
using FindEye.App.Helpers;
using FindEye.Core.Models;

namespace FindEye.App.Converters;

[ValueConversion(typeof(DiffType), typeof(string))]
public class DiffTypeNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DiffType dt)
        {
            return dt switch
            {
                DiffType.OnlyInA => TranslationManager.Instance["Diff_OnlyInA"],
                DiffType.OnlyInB => TranslationManager.Instance["Diff_OnlyInB"],
                DiffType.ContentDifferent => TranslationManager.Instance["Diff_ContentDiffers"],
                _ => TranslationManager.Instance["Diff_Identical"]
            };
        }
        return "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
