using Command;
using System;
using System.IO;
using System.Reflection;

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
    public string Template { get; set; } = Path.Combine(AppContext.BaseDirectory, "template.cs");

    [ShortOption('l')]
    [LongOption("template-level")]
    public int TemplateLevel { get; set; } = 1;

    [ShortOption('o')]
    [LongOption("output")]
    public string Output { get; set; } = "";

    [ShortOption('h')]
    [LongOption("help")]
    public void Help()
    {
        Console.WriteLine(
$@"Usage: {Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location)} [OPTION]... FILE
Generate LALR(1) parser.

  -i, --input FILE          input syntax definition file
  -o, --output DIRECTORY    output to DIRECTORY
  -t, --template DIRECTORY  specify the output programming language
  -l, --template-level N    specify the output range for template

  -h, --help                this help and exit
  -V, --version             ersion information and exit

contribute to https://github.com/zenuas/Yanp");
        Environment.Exit(0);
    }

    [ShortOption('V')]
    [LongOption("version")]
    public void Version()
    {
        Console.WriteLine(
$@"yanp {File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location):d}

Copyright zenuas
Released under the MIT license
https://github.com/zenuas/Yanp");
        Environment.Exit(0);
    }
}
