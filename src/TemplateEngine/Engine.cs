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
    public static void Run(Syntax syntax, Node[] nodes, Table[] tables, string source, TextWriter output)
    {
        var config = new TemplateServiceConfiguration()
        {
            Language = Language.CSharp,
            EncodedStringFactory = new RawStringFactory(),
            ReferenceResolver = new ReferenceResolver(),
        };
        var model = new Model
        {
            Syntax = syntax,
            Nodes = nodes,
            Tables = tables,
            GetDefine = (x, def) => syntax.Defines.TryGetValue(x, out var value) ? value : def,
            NodeToTable = (node) => tables.First(x => x.Node.Equals(node)),
        };
        RazorEngineService.Create(config).RunCompile(source, "templateKey", output, model.GetType(), model);
    }
}
