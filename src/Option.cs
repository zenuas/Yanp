using Command;
using System;
using System.IO;

namespace Yanp;

public class Option
{
    [ShortOption('i')]
    [LongOption("input")]
    public TextReader Input { get; set; } = Console.In;

    [ShortOption('v')]
    [LongOption("verbose")]
    public TextReader? Verbose { get; set; } = null;

    [ShortOption('t')]
    [LongOption("template")]
    public string Template { get; set; } = Path.Combine(AppContext.BaseDirectory, "cs");

    [ShortOption('o')]
    [LongOption("output")]
    public string Output { get; set; } = "";
}
