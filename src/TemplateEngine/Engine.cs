using Extensions;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System.IO;
using System.Linq;
using Yanp.Data;

namespace Yanp.TemplateEngine;

public static class Engine
{
    public static void Run(string basepath, Syntax syntax, Node[] nodes, Table[] tables, string source, TextWriter output)
    {
        var config = new TemplateServiceConfiguration()
        {
            Language = Language.CSharp,
            EncodedStringFactory = new RawStringFactory(),
            ReferenceResolver = new ReferenceResolver(),
            TemplateManager = new DelegateTemplateManager(key => File.ReadAllText(Path.Combine(basepath, key))),
        };
        var model = new Model
        {
            Syntax = syntax,
            Nodes = nodes,
            Tables = tables,
            GetDefine = (x, def) => syntax.Defines.TryGetValue(x, out var value) ? value : def,
            GetSymbols = () => syntax.Declares
                .Where(x => x.Key != "$ACCEPT")
                .Sort((a, b) =>
                    a.Value.Name.Type == Parser.Symbols.CHAR && b.Value.Name.Type == Parser.Symbols.VAR ? -1 :
                    a.Value.Name.Type == Parser.Symbols.VAR && b.Value.Name.Type == Parser.Symbols.CHAR ? 1 :
                    a.Value.Name.Type == Parser.Symbols.VAR && b.Value.Name.Type == Parser.Symbols.VAR && a.Value.IsTerminalSymbol && !b.Value.IsTerminalSymbol ? -1 :
                    a.Value.Name.Type == Parser.Symbols.VAR && b.Value.Name.Type == Parser.Symbols.VAR && !a.Value.IsTerminalSymbol && b.Value.IsTerminalSymbol ? 1 :
                    string.Compare(a.Value.Name.Value, b.Value.Name.Value))
                .Select(x => x.Key)
                .ToArray(),
            GetGrammarLines = () => syntax.Grammars.Values
                .Flatten()
                .Sort((a, b) =>
                    a.LineNumber < b.LineNumber ? -1 :
                    a.LineColumn < b.LineColumn ? -1 :
                    a.LineColumn - b.LineColumn)
                .ToArray(),
            NodeToTable = (node) => tables.First(x => x.Node.Equals(node)),
        };
        RazorEngineService.Create(config).RunCompile(source, "templateKey", output, model.GetType(), model);
    }
}
