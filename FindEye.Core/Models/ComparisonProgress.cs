namespace FindEye.Core.Models;

public class ComparisonProgress
{
    public string Stage { get; init; } = string.Empty;
    public int Percent { get; init; }
    public int FilesProcessed { get; init; }
    public int TotalFiles { get; init; }
    public string? CurrentFile { get; init; }
    public bool IsIndeterminate { get; init; }
    public int ErrorCount { get; init; }
}
