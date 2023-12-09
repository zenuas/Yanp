using Command;
using System;
using System.Collections.Generic;
using System.IO;

namespace Yanp;

public class Option
{
    [CommandOption('i')]
    [CommandOption("input")]
    public TextReader Input { get; init; } = Console.In;

    [CommandOption('v')]
    [CommandOption("verbose")]
    public void Verbose() => Exclude.Remove("v");

    public HashSet<string> Exclude { get; init; } = ["v"];

    [CommandOption('t')]
    [CommandOption("template")]
    public string Template { get; init; } = Path.Combine(AppContext.BaseDirectory, "template.cs");

    [CommandOption('l')]
    [CommandOption("template-level")]
    public int TemplateLevel { get; init; } = 1;

    [CommandOption('o')]
    [CommandOption("output")]
    public string Output { get; init; } = "";

    [CommandOption('h')]
    [CommandOption("help")]
    public static void Help()
    {
        Console.WriteLine(
$@"Usage: {Path.GetFileNameWithoutExtension(AppContext.BaseDirectory)} [OPTION]... FILE
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

    [CommandOption('V')]
    [CommandOption("version")]
    public static void Version()
    {
        Console.WriteLine(
$@"yanp {File.GetLastWriteTime(AppContext.BaseDirectory):d}

Copyright zenuas
Released under the MIT license
https://github.com/zenuas/Yanp");
        Environment.Exit(0);
    }
}
