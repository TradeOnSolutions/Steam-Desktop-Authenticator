using System.IO;

namespace TradeOnSda.Data.FileSystemAdapters;

public class WindowsFileSystemAdapter : FileSystemAdapterBase
{
    private readonly string _basePath;
    
    public WindowsFileSystemAdapter()
    {
        _basePath = Directory.GetCurrentDirectory();
    }
    
    protected override string GetBasePath()
    {
        return _basePath;
    }
}