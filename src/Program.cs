using Command;
using Extensions;
using System;
using System.IO;
using System.Linq;
using Yanp.Data;
using Yanp.TemplateEngine;

namespace Yanp;

public static class Program
{
    public static void Main(string[] args)
    {
        var opt = new Option();
        var xs = CommandLine.Run(opt, args);

        if (xs.Length > 1) throw new Exception($"extra operand '{xs[1]}'");
        if (xs.Length == 0)
        {
            Run(opt, opt.Input);
        }
        else
        {
            using var input = new StreamReader(xs[0]);
            Run(opt, input);
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

        Directory
            .GetFiles(opt.Template, "*.txt")
            .Select(x => (Path: x, TemplateLevel: Path.GetExtension(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(x))).Skip(1).ToStringByChars()))
            .Where(x => !int.TryParse(x.TemplateLevel, out var value) || value <= opt.TemplateLevel)
            .AsParallel()
            .ForAll(async x =>
            {
                var source = File.ReadAllTextAsync(x.Path);
                using var output = new StreamWriter(Path.Combine(opt.Output, Path.GetFileNameWithoutExtension(x.Path)));
                var config = Engine.CreateConfig(opt.Template);
                var model = Engine.CreateModel(syntax, nodes, tables);
                Engine.Run(config, model, await source, output);
            });
    }
}
