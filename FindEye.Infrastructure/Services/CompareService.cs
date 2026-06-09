using System.Diagnostics;
using FindEye.Core.Models;
using FindEye.Core.Services;

namespace FindEye.Infrastructure.Services;

public class CompareService : ICompareService
{
    private readonly IFileSystemService _fileSystem;
    private readonly IFileHashService _hashService;

    public CompareService(IFileSystemService fileSystem, IFileHashService hashService)
    {
        _fileSystem = fileSystem;
        _hashService = hashService;
    }

    public async Task<CompareResult> CompareFoldersAsync(
        string folderA, string folderB,
        IProgress<ComparisonProgress> progress, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        // Phase 1: Enumerate both folders in parallel
        progress.Report(new ComparisonProgress
        {
            Stage = "Progress_Enumerating",
            IsIndeterminate = true
        });

        var filesA = new List<FileItemInfo>();
        var filesB = new List<FileItemInfo>();

        var enumTaskA = Task.Run(async () =>
        {
            await foreach (var f in _fileSystem.EnumerateFilesAsync(folderA, ct))
                filesA.Add(f);
        }, ct);

        var enumTaskB = Task.Run(async () =>
        {
            await foreach (var f in _fileSystem.EnumerateFilesAsync(folderB, ct))
                filesB.Add(f);
        }, ct);

        await Task.WhenAll(enumTaskA, enumTaskB);

        // Phase 2: Classify by path and size
        progress.Report(new ComparisonProgress
        {
            Stage = "Progress_Classifying",
            IsIndeterminate = true
        });

        var dictA = filesA.ToDictionary(f => f.RelativePath, f => f, StringComparer.OrdinalIgnoreCase);
        var dictB = filesB.ToDictionary(f => f.RelativePath, f => f, StringComparer.OrdinalIgnoreCase);

        var result = new CompareResult
        {
            FolderA = folderA,
            FolderB = folderB,
            TotalFilesA = filesA.Count,
            TotalFilesB = filesB.Count,
            FilesInA = dictA,
            FilesInB = dictB,
            DiffMap = new Dictionary<string, DiffType>(StringComparer.OrdinalIgnoreCase)
        };

        var matchedForHashing = new List<(FileItemInfo fileA, FileItemInfo fileB)>();

        foreach (var (relPath, fileA) in dictA)
        {
            if (!dictB.TryGetValue(relPath, out var fileB))
            {
                result.DiffMap[relPath] = DiffType.OnlyInA;
                result.OnlyInACount++;
            }
            else if (fileA.Size != fileB.Size)
            {
                result.DiffMap[relPath] = DiffType.ContentDifferent;
                result.ContentDifferentCount++;
            }
            else
            {
                matchedForHashing.Add((fileA, fileB));
            }
        }

        foreach (var (relPath, _) in dictB)
        {
            if (!dictA.ContainsKey(relPath))
            {
                result.DiffMap[relPath] = DiffType.OnlyInB;
                result.OnlyInBCount++;
            }
        }

        // Phase 3: Hash files with same name and size
        var needsHash = new List<FileItemInfo>();
        foreach (var (fa, fb) in matchedForHashing)
        {
            needsHash.Add(fa);
            needsHash.Add(fb);
        }

        if (needsHash.Count > 0)
        {
            int parallelism = Math.Max(1, Environment.ProcessorCount - 1);
            await _hashService.ComputeHashesAsync(needsHash, parallelism, progress, ct);
        }

        // Phase 4: Final classification based on hashes
        foreach (var (fileA, fileB) in matchedForHashing)
        {
            string relPath = fileA.RelativePath;
            if (fileA.HashComputed && fileB.HashComputed &&
                string.Equals(fileA.Sha256Hash, fileB.Sha256Hash, StringComparison.OrdinalIgnoreCase))
            {
                result.DiffMap[relPath] = DiffType.Identical;
                result.IdenticalCount++;
            }
            else
            {
                result.DiffMap[relPath] = DiffType.ContentDifferent;
                result.ContentDifferentCount++;
            }
        }

        sw.Stop();
        result.Elapsed = sw.Elapsed;

        progress.Report(new ComparisonProgress
        {
            Stage = "Progress_Complete",
            Percent = 100,
            FilesProcessed = filesA.Count + filesB.Count,
            TotalFiles = filesA.Count + filesB.Count
        });

        return result;
    }
}
