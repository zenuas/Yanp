using System;

namespace Yanp.Parser;

public class SyntaxErrorException : Exception
{
    public required int LineNumber { get; init; }
    public required int LineColumn { get; init; }

    public SyntaxErrorException(string message) : base(message)
    {
    }
}
