using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using FindEye.App.Helpers;
using FindEye.Core.Models;
using FindEye.Core.Services;
using FindEye.Infrastructure.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Dialogs;

namespace FindEye.App.ViewModels;

public class CompareViewModel : BindableBase
{
    private readonly ICompareService _compareService;
    private readonly IExtractService _extractService;
    private readonly IDialogService _dialogService;
    private CancellationTokenSource? _cts;

    public CompareViewModel(
        ICompareService compareService,
        IExtractService extractService,
        IDialogService dialogService)
    {
        _compareService = compareService;
        _extractService = extractService;
        _dialogService = dialogService;

        BrowseFolderACommand = new DelegateCommand(() => BrowseFolder(path => FolderAPath = path));
        BrowseFolderBCommand = new DelegateCommand(() => BrowseFolder(path => FolderBPath = path));
        CompareCommand = new DelegateCommand(ExecuteCompareAsync, () => CanCompare)
            .ObservesProperty(() => FolderAPath)
            .ObservesProperty(() => FolderBPath)
            .ObservesProperty(() => IsComparing);
        CancelCommand = new DelegateCommand(ExecuteCancel, () => IsComparing)
            .ObservesProperty(() => IsComparing);
        ExtractCommand = new DelegateCommand(ExecuteExtractAsync, () => _lastResult != null && !IsComparing)
            .ObservesProperty(() => IsComparing);
    }

    // --- Folder paths ---
    private string _folderAPath = string.Empty;
    public string FolderAPath
    {
        get => _folderAPath;
        set { SetProperty(ref _folderAPath, value); CompareCommand.RaiseCanExecuteChanged(); }
    }

    private string _folderBPath = string.Empty;
    public string FolderBPath
    {
        get => _folderBPath;
        set { SetProperty(ref _folderBPath, value); CompareCommand.RaiseCanExecuteChanged(); }
    }

    public bool CanCompare =>
        !string.IsNullOrWhiteSpace(FolderAPath) &&
        !string.IsNullOrWhiteSpace(FolderBPath) &&
        !IsComparing;

    // --- Comparison state ---
    private bool _isComparing;
    public bool IsComparing
    {
        get => _isComparing;
        set
        {
            SetProperty(ref _isComparing, value);
            CompareCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            ExtractCommand.RaiseCanExecuteChanged();
        }
    }

    private int _progressPercent;
    public int ProgressPercent
    {
        get => _progressPercent;
        set => SetProperty(ref _progressPercent, value);
    }

    private string _progressText = string.Empty;
    public string ProgressText
    {
        get => _progressText;
        set => SetProperty(ref _progressText, value);
    }

    private bool _isIndeterminate;
    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        set => SetProperty(ref _isIndeterminate, value);
    }

    // --- Tree ---
    private List<CompareTreeNode>? _allTreeRoots;
    private List<CompareTreeNode> _treeRoots = new();
    public List<CompareTreeNode> TreeRoots
    {
        get => _treeRoots;
        set => SetProperty(ref _treeRoots, value);
    }

    private CompareTreeNode? _selectedItem;
    public CompareTreeNode? SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetProperty(ref _selectedItem, value);
            RaisePropertyChanged(nameof(HasSelection));
        }
    }

    public bool HasSelection => SelectedItem != null;

    private bool _hasResults;
    public bool HasResults
    {
        get => _hasResults;
        set => SetProperty(ref _hasResults, value);
    }

    private string _summaryText = string.Empty;
    public string SummaryText
    {
        get => _summaryText;
        set => SetProperty(ref _summaryText, value);
    }

    // --- Filter ---
    private bool _hideIdentical = true;
    public bool HideIdentical
    {
        get => _hideIdentical;
        set
        {
            SetProperty(ref _hideIdentical, value);
            ApplyFilter();
        }
    }

    // --- Commands ---
    public DelegateCommand BrowseFolderACommand { get; }
    public DelegateCommand BrowseFolderBCommand { get; }
    public DelegateCommand CompareCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public DelegateCommand ExtractCommand { get; }

    // --- Internal state ---
    private CompareResult? _lastResult;

    private static void BrowseFolder(Action<string> setter)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog();
        dialog.ShowNewFolderButton = false;
        dialog.UseDescriptionForTitle = true;
        dialog.Description = "Select folder";

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            setter(dialog.SelectedPath);
    }

    private async void ExecuteCompareAsync()
    {
        if (string.IsNullOrWhiteSpace(FolderAPath) || string.IsNullOrWhiteSpace(FolderBPath))
            return;

        IsComparing = true;
        HasResults = false;
        _lastResult = null;
        TreeRoots = new List<CompareTreeNode>();
        SummaryText = TranslationManager.Instance["StatusReady"];

        _cts = new CancellationTokenSource();
        var progress = new Progress<ComparisonProgress>(OnProgressReport);

        try
        {
            _lastResult = await Task.Run(
                () => _compareService.CompareFoldersAsync(
                    FolderAPath, FolderBPath, progress, _cts.Token),
                _cts.Token);

            _allTreeRoots = TreeBuilder.BuildTree(_lastResult);
            ApplyFilter();
            HasResults = true;

            var t = TranslationManager.Instance;
            SummaryText = string.Format(CultureInfo.CurrentUICulture,
                t["StatusComplete"],
                _lastResult.Elapsed.TotalSeconds,
                _lastResult.IdenticalCount,
                _lastResult.OnlyInACount,
                _lastResult.OnlyInBCount,
                _lastResult.ContentDifferentCount);
        }
        catch (OperationCanceledException)
        {
            SummaryText = TranslationManager.Instance["StatusCancelled"];
        }
        catch (Exception ex)
        {
            SummaryText = string.Format(TranslationManager.Instance["StatusError"], ex.Message);
        }
        finally
        {
            IsComparing = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void OnProgressReport(ComparisonProgress p)
    {
        ProgressPercent = p.Percent;
        IsIndeterminate = p.IsIndeterminate;

        if (!string.IsNullOrEmpty(p.CurrentFile))
            ProgressText = $"{p.Stage}: {p.CurrentFile} ({p.FilesProcessed}/{p.TotalFiles})";
        else
            ProgressText = p.Stage;
    }

    private void ExecuteCancel()
    {
        _cts?.Cancel();
    }

    private void ExecuteExtractAsync()
    {
        if (_lastResult == null) return;

        var parameters = new DialogParameters
        {
            { "result", _lastResult },
            { "folderA", FolderAPath },
            { "folderB", FolderBPath }
        };

        _dialogService.ShowDialog("ExtractDialog", parameters, async r =>
        {
            if (r.Result == ButtonResult.OK && r.Parameters.TryGetValue<ExtractOptions>("options", out var options))
            {
                IsComparing = true;
                _cts = new CancellationTokenSource();
                var progress = new Progress<ComparisonProgress>(OnProgressReport);

                try
                {
                    await Task.Run(
                        () => _extractService.ExtractFilesAsync(
                            _lastResult!, options, progress, _cts.Token),
                        _cts.Token);

                    SummaryText = string.Format(
                        TranslationManager.Instance["ExtractComplete"],
                        _lastResult!.DiffMap.Count(kvp =>
                        {
                            var dt = kvp.Value;
                            return (dt == DiffType.OnlyInA && options.IncludeOnlyInA) ||
                                   (dt == DiffType.OnlyInB && options.IncludeOnlyInB) ||
                                   (dt == DiffType.ContentDifferent && options.IncludeContentDifferent) ||
                                   (dt == DiffType.Identical && options.IncludeIdentical);
                        }));
                }
                catch (OperationCanceledException)
                {
                    SummaryText = TranslationManager.Instance["StatusCancelled"];
                }
                catch (Exception ex)
                {
                    SummaryText = string.Format(TranslationManager.Instance["StatusError"], ex.Message);
                }
                finally
                {
                    IsComparing = false;
                    _cts?.Dispose();
                    _cts = null;
                }
            }
        });
    }

    private void ApplyFilter()
    {
        if (_allTreeRoots == null)
        {
            TreeRoots = new List<CompareTreeNode>();
            return;
        }

        if (!HideIdentical)
        {
            TreeRoots = _allTreeRoots;
            return;
        }

        TreeRoots = FilterTree(_allTreeRoots);
    }

    private static List<CompareTreeNode> FilterTree(List<CompareTreeNode> nodes)
    {
        var result = new List<CompareTreeNode>();
        foreach (var node in nodes)
        {
            if (node.IsDirectory)
            {
                var filteredChildren = FilterTree(node.Children.ToList());
                if (filteredChildren.Count > 0)
                {
                    var clone = CloneDirectoryNode(node, filteredChildren);
                    result.Add(clone);
                }
            }
            else if (node.DiffType != DiffType.Identical)
            {
                result.Add(node);
            }
        }
        return result;
    }

    private static CompareTreeNode CloneDirectoryNode(CompareTreeNode original, List<CompareTreeNode> children)
    {
        return new CompareTreeNode
        {
            Name = original.Name,
            RelativePath = original.RelativePath,
            IsDirectory = true,
            DiffType = children.Any(c => c.DiffType != DiffType.Identical)
                ? DiffType.ContentDifferent
                : DiffType.Identical,
            Children = new ObservableCollection<CompareTreeNode>(children)
        };
    }
}
