using System;

namespace Parser;

public class SyntaxErrorException : Exception
{
    public int? LineNumber { get; init; }
    public int? LineColumn { get; init; }

    public SyntaxErrorException(string message) : base(message)
    {
    }
}
