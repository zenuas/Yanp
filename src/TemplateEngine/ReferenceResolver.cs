using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;
using System.Collections.Generic;

namespace Yanp.TemplateEngine;

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