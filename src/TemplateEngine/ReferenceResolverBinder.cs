using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;
using System;
using System.Collections.Generic;

namespace Yanp.TemplateEngine;

public class ReferenceResolverBinder : IReferenceResolver
{
    public required Func<TypeContext, IEnumerable<CompilerReference>?, IEnumerable<CompilerReference>> GetReferences;

    IEnumerable<CompilerReference> IReferenceResolver.GetReferences(TypeContext context, IEnumerable<CompilerReference>? includeAssemblies) => GetReferences(context, includeAssemblies);
}
