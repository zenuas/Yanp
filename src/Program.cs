using Command;
using Extensions;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
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

        Directory.GetFiles(opt.Template).AsParallel().ForAll(x => Output(opt, syntax, nodes, x));
    }

    public static void Output(Option opt, Syntax syntax, Node[] nodes, string path)
    {
        using var output = new StreamWriter(Path.Combine(opt.Output, Path.GetFileNameWithoutExtension(path)));
        var source = File.ReadAllText(path);
        var get_define = (string x, string def) => syntax.Defines.TryGetValue(x, out var value) ? value : def;

        var config = new TemplateServiceConfiguration()
        {
            Language = Language.CSharp,
            EncodedStringFactory = new RawStringFactory(),
            ReferenceResolver = new ReferenceResolver(),
        };
        var model = new Model { Option = opt, Syntax = syntax, Nodes = nodes, GetDefine = get_define };
        RazorEngineService.Create(config).RunCompile(source, "Yanp", output, model.GetType(), model);
    }

    public class Model
    {
        public required Option Option;
        public required Syntax Syntax;
        public required Node[] Nodes;
        public required Func<string, string, string> GetDefine;
    }

    public class ReferenceResolver : IReferenceResolver
    {
        public IEnumerable<CompilerReference> GetReferences(TypeContext context, IEnumerable<CompilerReference> includeAssemblies)
        {
            foreach (var x in new UseCurrentAssembliesReferenceResolver().GetReferences(context, includeAssemblies))
            {
                yield return x;
            }

            yield return CompilerReference.From(GetType().Assembly);
        }
    }
}
