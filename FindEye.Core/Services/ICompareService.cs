using FindEye.Core.Models;

namespace FindEye.Core.Services;

public interface ICompareService
{
    Task<CompareResult> CompareFoldersAsync(
        string folderA,
        string folderB,
        IProgress<ComparisonProgress> progress,
        CancellationToken ct);
}
