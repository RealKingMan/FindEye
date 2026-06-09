using FindEye.Core.Models;
using FindEye.Core.Services;

namespace FindEye.Infrastructure.Services;

public class ExtractService : IExtractService
{
    public async Task ExtractFilesAsync(
        CompareResult result, ExtractOptions options,
        IProgress<ComparisonProgress> progress, CancellationToken ct)
    {
        var sourceDict = options.UseAAsBase ? result.FilesInA : result.FilesInB;

        // Count files to extract
        int total = result.DiffMap.Count(kvp => ShouldInclude(kvp.Value, options));
        int completed = 0;

        foreach (var (relPath, diffType) in result.DiffMap)
        {
            if (!ShouldInclude(diffType, options))
                continue;

            ct.ThrowIfCancellationRequested();

            if (!sourceDict.TryGetValue(relPath, out var sourceItem))
                continue;

            var destFilePath = Path.Combine(options.DestinationPath, relPath);
            var destDir = Path.GetDirectoryName(destFilePath);
            if (!string.IsNullOrEmpty(destDir))
                Directory.CreateDirectory(destDir);

            await using var sourceStream = new FileStream(
                sourceItem.FullPath, FileMode.Open, FileAccess.Read,
                FileShare.Read, 81920, useAsync: true);
            await using var destStream = new FileStream(
                destFilePath, FileMode.Create, FileAccess.Write,
                FileShare.None, 81920, useAsync: true);
            await sourceStream.CopyToAsync(destStream, ct);

            int processed = Interlocked.Increment(ref completed);
            progress.Report(new ComparisonProgress
            {
                Stage = "Progress_Extracting",
                Percent = processed * 100 / total,
                FilesProcessed = processed,
                TotalFiles = total,
                CurrentFile = relPath
            });
        }
    }

    private static bool ShouldInclude(DiffType type, ExtractOptions o) => type switch
    {
        DiffType.OnlyInA => o.IncludeOnlyInA,
        DiffType.OnlyInB => o.IncludeOnlyInB,
        DiffType.ContentDifferent => o.IncludeContentDifferent,
        DiffType.Identical => o.IncludeIdentical,
        _ => false
    };
}
