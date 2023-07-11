using System.Threading;
using System.Threading.Tasks;

namespace TradeOnSda.Data.FileSystemAdapters;

public interface IFileSystemAdapter
{
    public Task<string> ReadFileAsync(string relativePath, CancellationToken cancellationToken);
    
    public Task WriteFileAsync(string relativePath, string content, CancellationToken cancellationToken);

    public bool ExistsFile(string relativePath);
}