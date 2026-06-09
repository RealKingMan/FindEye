using System;
using System.Globalization;
using System.Windows.Data;
using FindEye.App.Helpers;

namespace FindEye.App.Converters;

/// <summary>
/// Returns translated resource string when value is null, otherwise passes through.
/// </summary>
[ValueConversion(typeof(object), typeof(string))]
public class NullToTranslatedConverter : IValueConverter
{
    /// <summary>
    /// Resource key to use for null value. Defaults to "Common_NA".
    /// </summary>
    public string NullKey { get; set; } = "Common_NA";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return TranslationManager.Instance[NullKey];
        if (value is string s && string.IsNullOrEmpty(s))
            return TranslationManager.Instance[NullKey];
        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
