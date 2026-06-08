namespace FindEye.Core.Models;

public class FileItemInfo
{
    public string RelativePath { get; init; } = string.Empty;
    public string FullPath { get; init; } = string.Empty;
    public long Size { get; init; }
    public DateTime LastWriteTime { get; init; }
    public string? Sha256Hash { get; set; }
    public bool HashComputed { get; set; }
    public string? ErrorMessage { get; set; }
}
