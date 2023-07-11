using System;
using System.IO;

namespace TradeOnSda.Data.FileSystemAdapters;

public class MacFileSystemAdapter : FileSystemAdapterBase
{
    protected override string GetBasePath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "TradeOn SDA");
    }
}