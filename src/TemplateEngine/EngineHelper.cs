using Mina.Extension;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Yanp.TemplateEngine;

public static class EngineHelper
{
    public static void Run(ITemplateServiceConfiguration config, object model, string source, TextWriter output) => RazorEngineService.Create(config).RunCompile(source, "templateKey", output, model.GetType(), model);

    public static IEnumerable<CompilerReference> GetAssemlyLoads(string source) => source
        .SplitLine()
        .TakeWhile(x => x.StartsWith("//#Assembly.Load "))
        .Select(x => x[("//#Assembly.Load ".Length)..])
        .Select(x => CompilerReference.From(Assembly.Load(x)));

    public static string TrimHeader(string source) => source
        .SplitLine()
        .SkipWhile(x => x.StartsWith("//#Assembly.Load "))
        .Join("\r\n");
}
