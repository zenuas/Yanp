using Extensions;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using Yanp.Data;

namespace Yanp;

public static class LALR1
{
    public static void Generate(Syntax syntax, Node[] nodes)
    {
        var lines = GrammarLines(nodes);
        var nullable = Nullable(lines);
        var head = Head(syntax, lines);
        var follow = Follow(nodes, lines, nullable);
        nodes.Each(node =>
        {
            var ahead = Ahead(syntax, node, nullable, head);
            var followable = Followable(syntax, node, follow, nullable, ahead);
            Lookahead(syntax, node, followable);
        });
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
                if (line.Grammars.All(x => nullable.Contains(x.Value)))
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

    public static Dictionary<string, HashSet<string>> Head(Syntax syntax, GrammarLine[] lines)
    {
        // s : a b | c => {Key: s, Value: {a, c}}
        var head_first = lines
            .Where(x => x.Grammars.Count > 0)
            .Select(x => (Head: x.Name.Value, First: x.Grammars.First().Value))
            .GroupBy(x => x.Head, x => x.First)
            .ToDictionary(x => x.Key, x => x.ToHashSet());

        var head = lines
            .Select(x => x.Name.Value)
            .Distinct()
            .ToDictionary(x => x, _ => new HashSet<string>());

        while (true)
        {
            var retry = false;

            head_first.Each(kv => kv.Value
                .Where(x => syntax.Declares[x].IsTerminalSymbol)
                .Each(x => retry = head[kv.Key].Add(x) || retry));

            head_first.Each(kv => kv.Value
                .ToArray()
                .Where(x => head_first.ContainsKey(x))
                .Select(x => head_first[x])
                .Flatten()
                .Each(x => retry = kv.Value.Add(x) || retry));

            if (!retry) break;
        }

        return head;
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

    public static Dictionary<string, HashSet<string>> Ahead(Syntax syntax, Node node, HashSet<string> nullable, Dictionary<string, HashSet<string>> head)
    {
        var reduceable = node.Lines
            .Where(x => x.Line.Grammars.Skip(x.Index).All(x => nullable.Contains(x.Value)))
            .Select(x => x.Line.Name.Value)
            .ToHashSet();

        var ahead = new Dictionary<string, HashSet<string>>();
        foreach (var look in node.Lines.Where(x => x.Index < x.Line.Grammars.Count && reduceable.Contains(x.Line.Grammars[x.Index].Value)))
        {
            var top = look.Line.Grammars[look.Index].Value;
            if (!ahead.ContainsKey(top)) ahead.Add(top, new());

            for (var i = look.Index + 1; i < look.Line.Grammars.Count; i++)
            {
                var s = look.Line.Grammars[i].Value;
                if (syntax.Declares[s].IsTerminalSymbol)
                {
                    _ = ahead[top].Add(s);
                    break;
                }
                else
                {
                    head[s].Each(x => ahead[top].Add(x));
                    if (!nullable.Contains(s)) break;
                }
            }
        }
        return ahead;
    }

    public static Dictionary<string, HashSet<string>> Followable(Syntax syntax, Node node, Dictionary<string, HashSet<string>> follow, HashSet<string> nullable, Dictionary<string, HashSet<string>> ahead)
    {
        var reduces = node.Lines
            .Where(x => x.Line.Grammars.Skip(x.Index).All(x => nullable.Contains(x.Value)))
            .ToArray();

        var followable = reduces
            .Where(x => x.Index > 0 || x.Line.Name.Value == syntax.Start)
            .GroupBy(x => x.Line.Name.Value, x => follow.TryGetValue(x.Line.Name.Value, out var value) ? value : new())
            .ToDictionary(x => x.Key, x => x.Flatten().ToHashSet());

        while (true)
        {
            var retry = false;

            reduces
                .Where(x => x.Index < x.Line.Grammars.Count)
                .Each(index =>
                {
                    var top = index.Line.Name.Value;
                    var current = index.Line.Grammars[index.Index].Value;
                    ahead.GetOrDefault(top)?.Each(x => retry = followable.GetOrNew(current).Add(x) || retry);
                    followable.GetOrDefault(top)?.Each(x => retry = followable.GetOrNew(current).Add(x) || retry);
                });

            if (!retry) break;
        }
        return followable;
    }

    public static void Lookahead(Syntax syntax, Node node, Dictionary<string, HashSet<string>> followable)
    {
        node.Lines
            .Where(x => x.Index >= x.Line.Grammars.Count)
            .Each(index => followable.GetOrDefault(index.Line.Name.Value)?.Each(x => node.Lookahead.GetOrNew(index).Add(syntax.Declares[x].Name)));
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
            }));

        return new() { Index = index, Node = node, Conflicts = conflicts.ToArray(), Actions = actions };
    }
}
