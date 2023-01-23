using Extensions;
using System.Collections.Generic;
using System.Linq;
using Yanp.Data;

namespace Yanp;

public static class LALR1
{
    public static void Generate(Node[] nodes)
    {
    }

    public static HashSet<string> Nullable(Node[] nodes)
    {
        var lines = nodes
            .Select(x => x.Lines)
            .Flatten()
            .Select(x => x.Line)
            .Distinct()
            .ToArray();

        var nullable = lines
            .Where(x => x.Grammars.IsEmpty())
            .Select(x => x.Name)
            .ToHashSet();

        while (true)
        {
            var retry = false;
            foreach (var line in lines.Where(x => !nullable.Contains(x.Name)))
            {
                if (line.Grammars.Where(x => !nullable.Contains(x.Value)).IsEmpty())
                {
                    _ = nullable.Add(line.Name);
                    retry = true;
                    break;
                }
            }
            if (!retry) break;
        }
        return nullable;
    }
}
