using System;

namespace Yanp.Data;

public class ParseException : Exception
{
    public ParseException(string message) : base(message)
    {
    }
}
