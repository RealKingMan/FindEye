using System.Runtime.CompilerServices;
using FindEye.Core.Models;
using FindEye.Core.Services;

namespace FindEye.Infrastructure.Services;

public class FileSystemService : IFileSystemService
{
    public async IAsyncEnumerable<FileItemInfo> EnumerateFilesAsync(
        string folderPath,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (!Directory.Exists(folderPath))
            yield break;

        foreach (var filePath in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();

            FileItemInfo item;
            try
            {
                var fi = new FileInfo(filePath);
                item = new FileItemInfo
                {
                    RelativePath = Path.GetRelativePath(folderPath, filePath),
                    FullPath = filePath,
                    Size = fi.Length,
                    LastWriteTime = fi.LastWriteTimeUtc
                };
            }
            catch (Exception ex)
            {
                item = new FileItemInfo
                {
                    RelativePath = Path.GetRelativePath(folderPath, filePath),
                    FullPath = filePath,
                    ErrorMessage = ex.Message
                };
            }

            yield return item;
        }
    }
}
