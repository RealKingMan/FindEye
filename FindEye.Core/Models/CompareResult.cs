namespace FindEye.Core.Models;

public class CompareResult
{
    public string FolderA { get; init; } = string.Empty;
    public string FolderB { get; init; } = string.Empty;
    public int TotalFilesA { get; init; }
    public int TotalFilesB { get; init; }
    public int IdenticalCount { get; set; }
    public int OnlyInACount { get; set; }
    public int OnlyInBCount { get; set; }
    public int ContentDifferentCount { get; set; }
    public int ErrorCount { get; set; }
    public TimeSpan Elapsed { get; set; }

    public Dictionary<string, FileItemInfo> FilesInA { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, FileItemInfo> FilesInB { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, DiffType> DiffMap { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
