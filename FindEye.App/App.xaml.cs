using System.Windows;
using FindEye.App.Helpers;
using FindEye.App.ViewModels;
using FindEye.App.Views;
using FindEye.Core.Services;
using FindEye.Infrastructure.Services;
using Prism.Ioc;
using Prism.Unity;

namespace FindEye.App;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
        => Container.Resolve<MainWindow>();
  //  ───────────┬───────────────────────────────────┐
  //│  颜色名   │               效果                │
  //├───────────┼───────────────────────────────────┤
  //│ Blue      │ 标准蓝                            │
  //├───────────┼───────────────────────────────────┤
  //│ LightBlue │ 浅蓝                              │
  //├───────────┼───────────────────────────────────┤
  //│ Cyan      │ 青色                              │
  //├───────────┼───────────────────────────────────┤
  //│ SkyBlue   │ 天蓝（MaterialDesignColors 扩展） │
  //├───────────┼───────────────────────────────────┤
  //│ Indigo    │ 靛蓝                   
    protected override void RegisterTypes(IContainerRegistry container)
    {
        container.RegisterSingleton<IFileSystemService, FileSystemService>();
        container.RegisterSingleton<IFileHashService, FileHashService>();
        container.RegisterSingleton<ICompareService, CompareService>();
        container.RegisterSingleton<IExtractService, ExtractService>();

        container.RegisterSingleton<MainWindowViewModel>();
        container.Register<CompareViewModel>();
        container.RegisterDialog<ExtractDialog, ExtractDialogViewModel>();

        container.RegisterForNavigation<CompareView, CompareViewModel>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        TranslationManager.Instance.SetCulture("en-US");
    }

    protected override void InitializeShell(Window shell)
    {
        base.InitializeShell(shell);
        var rm = Container.Resolve<Prism.Navigation.Regions.IRegionManager>();
        rm.RequestNavigate("MainRegion", "CompareView");
    }
}
