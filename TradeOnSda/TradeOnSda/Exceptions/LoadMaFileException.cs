using System;

namespace TradeOnSda.Exceptions;

public class LoadMaFileException : Exception
{
    public LoadMaFileException(string message)
        : base(message)
    {
    }
}