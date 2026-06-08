using FindEye.Core.Models;

namespace FindEye.Core.Services;

public interface IFileSystemService
{
    IAsyncEnumerable<FileItemInfo> EnumerateFilesAsync(string folderPath, CancellationToken ct);
}
