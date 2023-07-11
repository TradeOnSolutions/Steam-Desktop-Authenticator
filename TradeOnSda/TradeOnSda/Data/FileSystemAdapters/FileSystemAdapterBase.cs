using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TradeOnSda.Data.FileSystemAdapters;

public abstract class FileSystemAdapterBase : IFileSystemAdapter
{
    protected abstract string GetBasePath();

    public async Task<string> ReadFileAsync(string relativePath, CancellationToken cancellationToken)
    {
        var path = Path.Combine(GetBasePath(), relativePath);

        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public async Task WriteFileAsync(string relativePath, string content, CancellationToken cancellationToken)
    {
        var path = Path.Combine(GetBasePath(), relativePath);

        var directory = Path.GetDirectoryName(path)!;

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(path, content, cancellationToken);
    }

    public bool ExistsFile(string relativePath) =>
        File.Exists(Path.Combine(GetBasePath(), relativePath));
}