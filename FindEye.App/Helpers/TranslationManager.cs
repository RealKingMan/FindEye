using System.ComponentModel;
using System.Globalization;

namespace FindEye.App.Helpers;

public class TranslationManager : INotifyPropertyChanged
{
    public static TranslationManager Instance { get; } = new();

    private static readonly System.Resources.ResourceManager ResourceManager =
        new("FindEye.App.Resources.UIStrings", typeof(TranslationManager).Assembly);

    public string this[string key]
    {
        get
        {
            var value = ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
            return value ?? $"[{key}]";
        }
    }

    public void SetCulture(string name)
    {
        var culture = new CultureInfo(name);
        CultureInfo.CurrentUICulture = culture;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(System.Windows.Data.Binding.IndexerName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
