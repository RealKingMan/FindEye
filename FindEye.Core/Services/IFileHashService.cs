using FindEye.Core.Models;

namespace FindEye.Core.Services;

public interface IFileHashService
{
    Task<string> ComputeHashAsync(string filePath, CancellationToken ct);

    Task ComputeHashesAsync(
        IList<FileItemInfo> files,
        int maxDegreeOfParallelism,
        IProgress<ComparisonProgress> progress,
        CancellationToken ct);
}
