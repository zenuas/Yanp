using System;

namespace Yanp;

public class ParseException : Exception
{
    public ParseException(string message) : base(message)
    {
    }
}
