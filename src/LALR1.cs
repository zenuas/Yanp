using Extensions;
using Parser;
using System.Collections.Generic;
using System.Linq;
using Yanp.Data;

namespace Yanp;

public static class LALR1
{
    public static void Generate(Syntax syntax, Node[] nodes)
    {
        var lines = GrammarLines(nodes);
        var follow = Follow(nodes, lines, Nullable(lines));
        nodes.Each(x => Lookahead(syntax, x, follow));
    }

    public static GrammarLine[] GrammarLines(Node[] nodes) => nodes
        .Select(x => x.Lines)
        .Flatten()
        .Select(x => x.Line)
        .Distinct()
        .ToArray();

    public static HashSet<string> Nullable(GrammarLine[] lines)
    {
        var nullable = lines
            .Where(x => x.Grammars.IsEmpty())
            .Select(x => x.Name.Value)
            .ToHashSet();

        while (true)
        {
            var retry = false;
            foreach (var line in lines.Where(x => !nullable.Contains(x.Name.Value)))
            {
                if (line.Grammars.Where(x => !nullable.Contains(x.Value)).IsEmpty())
                {
                    _ = nullable.Add(line.Name.Value);
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
            .Select(x => x.Name.Value)
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
            .Where(x => nonterminal_symbols.Contains(x.Name.Value))
            .GroupBy(x => x.Name.Value)
            .Select(x => (Name: x.Key, Nexts: x.Select(y => y.Nexts).Flatten()))
            .ToDictionary(x => x.Name, node => node.Nexts.Select(x => x.Name.Value).Where(x => !nonterminal_symbols.Contains(x)).ToHashSet());

        while (true)
        {
            var retry = false;
            last_nonterminal_lines.Each(x => follow[x.Head.Value].Each(y => retry = follow[x.Last].Add(y) || retry));
            next_nullable_lines.Each(x => follow[x.Next].Each(y => retry = follow[x.Current].Add(y) || retry));
            if (!retry) break;
        }

        return follow;
    }

    public static void Lookahead(Syntax syntax, Node node, Dictionary<string, HashSet<string>> follow)
    {
        node.Lines
            .Where(x => x.Line.Grammars.Count == x.Index && follow.ContainsKey(x.Line.Name.Value))
            .Each(line => node.Lookahead.Add(line, follow[line.Line.Name.Value].Select(x => syntax.Declares[x].Name).ToHashSet()));
    }

    public static Table CreateTables(Syntax syntax, Node node, int index)
    {
        var actions = node.Nexts.ToDictionary(x => x.Name, x => (IParserAction)new ShiftAction
        {
            Next = x,
            Priority = x.Lines
                .Where(y => y.Index > 0 && y.Line.Grammars[y.Index - 1].Value == x.Name.Value)
                .Select(y => y.Line.Priority)
                .Max(),
        });
        var reduces = node.Lines.Where(x => x.Index >= x.Line.Grammars.Count).ToArray();
        var conflicts = new List<string>();

        bool add_reduce(Token name, GrammarLine reduce)
        {
            var act = actions.FirstOrDefault(x => x.Key.Value == name.Value);
            if (act.Value is null)
            {
                actions.Add(name, new ReduceAction { Reduce = reduce });
            }
            else if (act.Value is ReduceAction p)
            {
                conflicts.Add($"reduce/reduce conflict ([reduce] {p.Reduce}, [reduce] {reduce})");
                return false;
            }
            else
            {
                var shift = act.Value.Cast<ShiftAction>();
                switch (
                    reduce.Priority == shift.Priority && reduce.Priority == 0 ? AssocTypes.Type :
                    reduce.Priority > shift.Priority ? AssocTypes.Left :
                    reduce.Priority < shift.Priority ? AssocTypes.Right :
                    reduce.Assoc)
                {
                    case AssocTypes.Left:
                        _ = actions.Remove(act.Key);
                        actions.Add(name, new ReduceAction { Reduce = reduce });
                        break;

                    case AssocTypes.Right:
                        return false;

                    case AssocTypes.Nonassoc:
                        conflicts.Add($"nonassociative ([shift] {shift.Next.Name}, [reduce] {reduce.Name})");
                        return false;

                    default:
                        conflicts.Add($"shift/reduce conflict ([shift] {shift.Next.Name}, [reduce] {reduce.Name})");
                        return false;
                }
            }
            return true;
        }

        reduces
            .Where(x => !node.Lookahead.ContainsKey(x) || node.Lookahead[x].IsEmpty())
            .Each(x => syntax.Declares.Values.Where(y => y.IsTerminalSymbol).Each(y => add_reduce(y.Name, x.Line)));

        reduces
            .Where(x => node.Lookahead.ContainsKey(x) && !node.Lookahead[x].IsEmpty())
            .Each(x => node.Lookahead[x].ToList().Each(y =>
                {
                    if (add_reduce(y, x.Line))
                    {
                        _ = node.Nexts.RemoveWhere(n => n.Name.Value == y.Value);
                    }
                    else
                    {
                        _ = node.Lookahead[x].Remove(y);
                    }
                })
            );

        return new Table { Index = index, Node = node, Conflicts = conflicts.ToArray(), Actions = actions };
    }
}
