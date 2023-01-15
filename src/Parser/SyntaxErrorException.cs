using System;

namespace Parser;

public class SyntaxErrorException : Exception
{
    public required int LineNumber { get; init; }
    public required int LineColumn { get; init; }

    public SyntaxErrorException(string message) : base(message)
    {
    }
}
