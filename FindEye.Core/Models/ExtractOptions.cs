namespace FindEye.Core.Models;

public class ExtractOptions
{
    public bool IncludeOnlyInA { get; set; } = true;
    public bool IncludeOnlyInB { get; set; } = true;
    public bool IncludeContentDifferent { get; set; } = true;
    public bool IncludeIdentical { get; set; }
    public string SourceBasePath { get; set; } = string.Empty;
    public bool UseAAsBase { get; set; } = true;
    public string DestinationPath { get; set; } = string.Empty;
}
