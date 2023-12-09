using Command;
using Extensions;
using System;
using System.IO;
using System.Linq;
using Yanp.TemplateEngine;

namespace Yanp;

public static class Program
{
    public static void Main(string[] args)
    {
        var (opt, xargs) = CommandLine.Run<Option>(args);

        if (xargs.Length > 1) throw new Exception($"extra operand '{xargs[1]}'");
        if (xargs.Length == 0)
        {
            Run(opt, opt.Input);
        }
        else
        {
            using var input = new StreamReader(xargs[0]);
            Run(opt, input);
        }
    }

    public static void Run(Option opt, TextReader input)
    {
        var syntax = SyntaxParser.Parse(input);

        if (syntax.Grammars.IsEmpty()) throw new Exception("no grammar has been specified");
        if (!syntax.Grammars.Contains(x => x.Key == syntax.Start)) throw new Exception("no grammar has been specified");

        var nodes = LR0.Generate(syntax);
        LALR1.Generate(syntax, nodes);
        var tables = nodes.Select((x, i) => LALR1.CreateTables(syntax, x, i)).ToArray();

        Directory
            .GetFiles(opt.Template, "*.txt")
            .Select(x => (Path: x, TemplateLevel: Path.GetExtension(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(x))).Skip(1).ToStringByChars()))
            .Where(x => !opt.Exclude.Contains(x.TemplateLevel) && (!int.TryParse(x.TemplateLevel, out var value) || value <= opt.TemplateLevel))
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
