using FindEye.Core.Models;

namespace FindEye.Core.Services;

public interface IExtractService
{
    Task ExtractFilesAsync(
        CompareResult result,
        ExtractOptions options,
        IProgress<ComparisonProgress> progress,
        CancellationToken ct);
}
