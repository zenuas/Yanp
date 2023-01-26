using Extensions;
using System.Collections.Generic;
using System.Linq;
using Yanp.Data;

namespace Yanp;

public static class LALR1
{
    public static void Generate(Syntax syntax, Node[] nodes)
    {
        var lines = GrammarLines(nodes);
        var follow = Follow(nodes, lines, Nullable(nodes, lines));
        Lookahead(syntax, nodes, follow);
    }

    public static GrammarLine[] GrammarLines(Node[] nodes) => nodes
        .Select(x => x.Lines)
        .Flatten()
        .Select(x => x.Line)
        .Distinct()
        .ToArray();

    public static HashSet<string> Nullable(Node[] nodes, GrammarLine[] lines)
    {
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

    public static Dictionary<string, HashSet<string>> Follow(Node[] nodes, GrammarLine[] lines, HashSet<string> nullable)
    {
        var nonterminal_symbols = lines
            .Select(x => x.Name)
            .Distinct()
            .Where(x => x != "$ACCEPT")
            .ToArray();

        // s : a b => (Head: s, Last: b)
        var last_nonterminal_lines = lines
            .Where(x => !x.Grammars.IsEmpty())
            .Select(x => (Head: x.Name, Last: x.Grammars.Last().Value))
            .Where(x => nonterminal_symbols.Contains(x.Last))
            .Distinct()
            .ToArray();

        // s : . a void b => (Current: a, Next: void)
        var next_nullable_lines = nodes
            .Select(x => x.Lines)
            .Flatten()
            .Where(x => x.Index <= x.Line.Grammars.Count - 2)
            .Select(x => (Current: x.Line.Grammars[x.Index].Value, Next: x.Line.Grammars[x.Index + 1].Value))
            .Where(x => nonterminal_symbols.Contains(x.Current) && nullable.Contains(x.Next))
            .Distinct()
            .ToArray();

        var follow = nodes
            .Where(x => nonterminal_symbols.Contains(x.Name))
            .GroupBy(x => x.Name)
            .Select(x => (Name: x.Key, Nexts: x.Select(y => y.Nexts).Flatten()))
            .ToDictionary(x => x.Name, node => node.Nexts.Select(x => x.Name).Where(x => !nonterminal_symbols.Contains(x)).ToHashSet());

        while (true)
        {
            var retry = false;
            last_nonterminal_lines.Each(x => follow[x.Head].Each(y => retry = follow[x.Last].Add(y) || retry));
            next_nullable_lines.Each(x => follow[x.Next].Each(y => retry = follow[x.Current].Add(y) || retry));
            if (!retry) break;
        }

        return follow;
    }

    public static void Lookahead(Syntax syntax, Node[] nodes, Dictionary<string, HashSet<string>> follow)
    {
        nodes.Each(node => node.Lines
            .Where(x => x.Line.Grammars.Count == x.Index && follow.ContainsKey(x.Line.Name))
            .Each(line => follow[line.Line.Name].Each(x => line.Lookahead.Add(syntax.Declares[x].Name))));
    }
}
