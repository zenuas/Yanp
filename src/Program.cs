using Command;
using Extensions;
using System;
using System.IO;
using System.Linq;
using Yanp.Data;

namespace Yanp;

public static class Program
{
    public static void Main(string[] args)
    {
        var opt = new Option();
        var xs = CommandLine.Run(opt, args);

        if (xs.Length == 0)
        {
            Run(opt, opt.Input);
        }
        else
        {
            xs.AsParallel().ForAll(x => Run(opt, new StreamReader(x)));
        }
    }

    public static void Run(Option opt, TextReader input)
    {
        var syntax = SyntaxParser.Parse(input);

        if (syntax.Grammars.IsEmpty()) throw new ParseException("no grammar has been specified");
        if (!syntax.Grammars.Contains(x => x.Key == syntax.Start)) throw new ParseException("no grammar has been specified");

        var nodes = LR0.Generate(syntax);
        LALR1.Generate(syntax, nodes);
        var tables = LALR1.CreateTables(syntax, nodes);

        Directory.GetFiles(opt.Template, "*.txt").AsParallel().ForAll(x =>
        {
            var source = File.ReadAllText(x);
            using var output = new StreamWriter(Path.Combine(opt.Output, Path.GetFileNameWithoutExtension(x)));
            TemplateEngine.Engine.Run(opt.Template, syntax, nodes, tables, source, output);
        });
    }
}
