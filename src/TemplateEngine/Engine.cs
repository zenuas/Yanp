using Extensions;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System.IO;
using System.Linq;
using System.Reflection;
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
            ReferenceResolver = new ReferenceResolverBinder
            {
                GetReferences = (context, includeAssemblies) => CompilerServicesUtility
                    .GetLoadedAssemblies()
                    .Where(a => !a.IsDynamic && File.Exists(a.Location) && !a.Location.Contains("CompiledRazorTemplates.Dynamic"))
                    .GroupBy(a => a.GetName().Name)
                    .Select(grp => grp.First(y => y.GetName().Version == grp.Max(x => x.GetName().Version)))
                    .Select(a => CompilerReference.From(a))
                    .Concat(includeAssemblies ?? Enumerable.Empty<CompilerReference>())
                    .Concat(new[] { CompilerReference.From(Assembly.Load("System.Text.RegularExpressions")) })
            },
            TemplateManager = new DelegateTemplateManager(key => File.ReadAllText(Path.Combine(basepath, key))),
        };
        var model = new Model
        {
            Syntax = syntax,
            Nodes = nodes,
            Tables = tables,
            GetDefine = (x, def) => syntax.Defines.TryGetValue(x, out var value) ? value : def,
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
                .Order((a, b) =>
                    a.LineNumber < b.LineNumber ? -1 :
                    a.LineColumn < b.LineColumn ? -1 :
                    a.LineColumn - b.LineColumn)
                .ToArray(),
            NodeToTable = (node) => tables.First(x => x.Node.Equals(node)),
        };
        RazorEngineService.Create(config).RunCompile(source, "templateKey", output, model.GetType(), model);
    }
}
