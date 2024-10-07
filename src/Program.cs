using Mina.Command;
using Mina.Extension;
using RazorEngine;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
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
            .ForAll(x =>
            {
                var source = File.ReadAllText(x.Path);
                using var output = new StreamWriter(Path.Combine(opt.Output, Path.GetFileNameWithoutExtension(x.Path)));
                var config = new TemplateServiceConfiguration()
                {
                    Language = Language.CSharp,
                    EncodedStringFactory = new RawStringFactory(),
                    ReferenceResolver = new ReferenceResolverBinder
                    {
                        GetReferences = (context, includeAssemblies) => new UseCurrentAssembliesReferenceResolver().GetReferences(context, includeAssemblies)
                            .Concat(EngineHelper.GetAssemlyLoads(source))
                    },
                    TemplateManager = new DelegateTemplateManager(x => File.ReadAllText(Path.Combine(opt.Template, x))),
                };
                var model = CreateModel(syntax, nodes, tables);
                EngineHelper.Run(config, model, source, output);
            });
    }

    public static Model CreateModel(Syntax syntax, Node[] nodes, Table[] tables)
    {
        return new()
        {
            Syntax = syntax,
            Nodes = nodes,
            Tables = tables,
            GetDefine = (name, default_value) => syntax.Defines.TryGetValue(name, out var value) ? value : default_value,
            GetSymbols = () => syntax.Declares
                .Order((a, b) =>
                    a.Value.Name.Type == Parser.Symbols.CHAR && b.Value.Name.Type == Parser.Symbols.VAR ? -1 :
                    a.Value.Name.Type == Parser.Symbols.VAR && b.Value.Name.Type == Parser.Symbols.CHAR ? 1 :
                    a.Value.Name.Type == Parser.Symbols.VAR && b.Value.Name.Type == Parser.Symbols.VAR && a.Value.IsTerminalSymbol && !b.Value.IsTerminalSymbol ? -1 :
                    a.Value.Name.Type == Parser.Symbols.VAR && b.Value.Name.Type == Parser.Symbols.VAR && !a.Value.IsTerminalSymbol && b.Value.IsTerminalSymbol ? 1 :
                    string.Compare(a.Value.Name.Value, b.Value.Name.Value))
                .Select(x => x.Value.Name)
                .ToArray(),
            GetGrammarLines = () => syntax.Grammars.Values
                .Flatten()
                .Order((a, b) => a.LineNumber != b.LineNumber ? a.LineNumber - b.LineNumber : a.LineColumn - b.LineColumn)
                .ToArray(),
            NodeToTable = (node) => tables.First(x => x.Node.Equals(node)),
        };
    }
}
