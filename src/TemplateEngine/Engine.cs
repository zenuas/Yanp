using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System.IO;
using Yanp.Data;

namespace Yanp.TemplateEngine;

public static class Engine
{
    public static void Run(Option opt, Syntax syntax, Node[] nodes, string path)
    {
        using var output = new StreamWriter(Path.Combine(opt.Output, Path.GetFileNameWithoutExtension(path)));
        var source = File.ReadAllText(path);

        var config = new TemplateServiceConfiguration()
        {
            Language = Language.CSharp,
            EncodedStringFactory = new RawStringFactory(),
            ReferenceResolver = new ReferenceResolver(),
        };
        var model = new Model
        {
            Option = opt,
            Syntax = syntax,
            Nodes = nodes,
            GetDefine = (x, def) => syntax.Defines.TryGetValue(x, out var value) ? value : def
        };
        RazorEngineService.Create(config).RunCompile(source, "templateKey", output, model.GetType(), model);
    }
}
