using System.Security.Cryptography;
using FindEye.Core.Models;
using FindEye.Core.Services;

namespace FindEye.Infrastructure.Services;

public class FileHashService : IFileHashService
{
    public async Task<string> ComputeHashAsync(string filePath, CancellationToken ct)
    {
        await using var stream = new FileStream(
            filePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, 4096, useAsync: true);

        var hash = await SHA256.HashDataAsync(stream, ct);
        return Convert.ToHexStringLower(hash);
    }

    public async Task ComputeHashesAsync(
        IList<FileItemInfo> files,
        int maxDegreeOfParallelism,
        IProgress<ComparisonProgress> progress,
        CancellationToken ct)
    {
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = ct
        };

        int completed = 0;
        int total = files.Count;

        await Parallel.ForEachAsync(files, parallelOptions, async (file, token) =>
        {
            try
            {
                file.Sha256Hash = await ComputeHashAsync(file.FullPath, token);
                file.HashComputed = true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                file.ErrorMessage = ex.Message;
            }

            int processed = Interlocked.Increment(ref completed);

            progress.Report(new ComparisonProgress
            {
                Stage = "Hashing",
                Percent = processed * 100 / total,
                FilesProcessed = processed,
                TotalFiles = total,
                CurrentFile = file.RelativePath
            });
        });
    }
}
