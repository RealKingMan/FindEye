using System.Windows;
using FindEye.App.Helpers;
using Prism.Commands;
using Prism.Mvvm;

namespace FindEye.App.ViewModels;

public class MainWindowViewModel : BindableBase
{
    public DelegateCommand ExitCommand { get; }
    public DelegateCommand<string> SwitchLanguageCommand { get; }

    public MainWindowViewModel()
    {
        ExitCommand = new DelegateCommand(() => Application.Current.Shutdown());
        SwitchLanguageCommand = new DelegateCommand<string>(OnSwitchLanguage);
    }

    private void OnSwitchLanguage(string culture)
    {
        TranslationManager.Instance.SetCulture(culture);
    }
}
