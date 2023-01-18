using Command;
using System.IO;

namespace Yanp;

public class Option
{
    [ShortOption('i')]
    [LongOption("input")]
    public TextReader Input { get; set; } = System.Console.In;

    [ShortOption('v')]
    [LongOption("verbose")]
    public TextReader? Verbose { get; set; } = null;
}
