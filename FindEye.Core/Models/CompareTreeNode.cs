using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FindEye.Core.Models;

public class CompareTreeNode : INotifyPropertyChanged
{
    public string Name { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool IsDirectory { get; init; }
    public DiffType DiffType { get; set; }
    public long Size { get; init; }
    public string? Sha256Hash { get; set; }
    public DateTime LastWriteTime { get; init; }
    public string? ErrorMessage { get; init; }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(); }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public ObservableCollection<CompareTreeNode> Children { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
