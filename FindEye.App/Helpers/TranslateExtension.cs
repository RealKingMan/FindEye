using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace FindEye.App.Helpers;

[ContentProperty(nameof(Key))]
public class TranslateExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    public TranslateExtension() { }

    public TranslateExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
            return string.Empty;

        var binding = new Binding("Item[]")
        {
            Source = TranslationManager.Instance,
            Path = new PropertyPath($"[{Key}]"),
            Mode = BindingMode.OneWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        return binding.ProvideValue(serviceProvider);
    }
}
