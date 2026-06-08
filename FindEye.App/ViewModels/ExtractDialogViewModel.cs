using System.Windows;
using FindEye.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Dialogs;

namespace FindEye.App.ViewModels;

public class ExtractDialogViewModel : BindableBase, IDialogAware
{
    public string Title => "Extract Files";

    public DialogCloseListener RequestClose { get; }

    private CompareResult? _result;
    private string _folderA = string.Empty;
    private string _folderB = string.Empty;

    // --- Options ---
    private bool _includeOnlyInA = true;
    public bool IncludeOnlyInA
    {
        get => _includeOnlyInA;
        set => SetProperty(ref _includeOnlyInA, value);
    }

    private bool _includeOnlyInB = true;
    public bool IncludeOnlyInB
    {
        get => _includeOnlyInB;
        set => SetProperty(ref _includeOnlyInB, value);
    }

    private bool _includeContentDifferent = true;
    public bool IncludeContentDifferent
    {
        get => _includeContentDifferent;
        set => SetProperty(ref _includeContentDifferent, value);
    }

    private bool _includeIdentical;
    public bool IncludeIdentical
    {
        get => _includeIdentical;
        set => SetProperty(ref _includeIdentical, value);
    }

    // --- Base folder ---
    public List<string> BaseFolderOptions { get; } = new() { "Folder A", "Folder B" };

    private string _selectedBaseFolder = "Folder A";
    public string SelectedBaseFolder
    {
        get => _selectedBaseFolder;
        set => SetProperty(ref _selectedBaseFolder, value);
    }

    private string _destinationPath = string.Empty;
    public string DestinationPath
    {
        get => _destinationPath;
        set => SetProperty(ref _destinationPath, value);
    }

    // --- Commands ---
    public DelegateCommand BrowseDestinationCommand { get; }
    public DelegateCommand ExtractCommand { get; }
    public DelegateCommand CancelCommand { get; }

    public ExtractDialogViewModel()
    {
        BrowseDestinationCommand = new DelegateCommand(ExecuteBrowseDestination);
        ExtractCommand = new DelegateCommand(ExecuteExtract);
        CancelCommand = new DelegateCommand(() =>
            RequestClose.Invoke(ButtonResult.Cancel));
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        _result = parameters.GetValue<CompareResult>("result");
        _folderA = parameters.GetValue<string>("folderA");
        _folderB = parameters.GetValue<string>("folderB");
    }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    private void ExecuteBrowseDestination()
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            ShowNewFolderButton = true,
            UseDescriptionForTitle = true,
            Description = "Select destination folder"
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            DestinationPath = dialog.SelectedPath;
    }

    private void ExecuteExtract()
    {
        if (string.IsNullOrWhiteSpace(DestinationPath))
        {
            MessageBox.Show("Please select a destination folder.", "FindEye",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var options = new ExtractOptions
        {
            IncludeOnlyInA = IncludeOnlyInA,
            IncludeOnlyInB = IncludeOnlyInB,
            IncludeContentDifferent = IncludeContentDifferent,
            IncludeIdentical = IncludeIdentical,
            UseAAsBase = SelectedBaseFolder == "Folder A",
            SourceBasePath = SelectedBaseFolder == "Folder A" ? _folderA : _folderB,
            DestinationPath = DestinationPath
        };

        var parameters = new DialogParameters { { "options", options } };
        RequestClose.Invoke(parameters, ButtonResult.OK);
    }
}
