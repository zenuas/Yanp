using Extensions;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using Yanp.Data;

namespace Yanp;

public static class LR0
{
    public static Node[] Generate(Syntax syntax)
    {
        AddAccept(syntax);
        var nodes = CreateNodes(syntax);
        Next(nodes, First(nodes));
        var merged = Merge(nodes);
        return Sweep(merged.First(x => x.Name == "$ACCEPT" && x.Lines[0].Index == 0));
    }

    public static void AddAccept(Syntax syntax)
    {
        var accept = new Token { Type = Symbols.VAR, LineNumber = 0, LineColumn = 0, Value = "$ACCEPT" };
        var end = new Token { Type = Symbols.VAR, LineNumber = 0, LineColumn = 0, Value = "$END" };

        syntax.Declares.Add(accept.Value, new Declarate { Name = accept.Value, Assoc = AssocTypes.Type, IsTerminalSymbol = false });
        syntax.Declares.Add(end.Value, new Declarate { Name = end.Value, Assoc = AssocTypes.Type });

        // $ACCEPT : syntax.Start $END
        syntax.Grammars.Add(accept.Value, new() {
            new() {
                Grammars = new() {
                    new() { Type = Symbols.VAR, LineNumber = 0, LineColumn = 0, Value = syntax.Start },
                    end
                }
            }
        });
    }

    public static Node[] CreateNodes(Syntax syntax)
    {
        return syntax.Grammars.Select(g =>
            g.Value.Select(gl =>
                {
                    var line = Lists.Sequence(0)
                        .Select(i => new Node { Name = i == 0 ? g.Key : gl.Grammars[i - 1].Value, Lines = new() { new() { Name = g.Key, Index = i, Line = gl } } })
                        .Take(gl.Grammars.Count + 1)
                        .ToArray();

                    line.Take(gl.Grammars.Count).Each((x, i) => x.Nexts.Add(line[i + 1]));
                    return line;
                }))
            .Flatten()
            .Flatten()
            .ToArray();
    }

    public static Node[] First(Node[] nodes)
    {
        return nodes
            .Where(x => x.Lines[0].Index == 0)
            .ToArray();
    }

    public static void Next(Node[] nodes, Node[] first)
    {
        while (true)
        {
            var retry = false;
            foreach (var node in nodes)
            {
                foreach (var head in node.Nexts
                    .Select(x => first.Where(y => y.Name == x.Name))
                    .Flatten()
                    .ToArray())
                {
                    head.Lines.Each(x => { if (!node.Lines.Contains(x)) node.Lines.Add(x); });
                    head.Nexts.Each(x => retry = node.Nexts.Add(x) || retry);
                }
            }
            if (!retry) break;
        }
    }

    public static Node[] Merge(Node[] nodes)
    {
        var merged = nodes.ToList();
        while (true)
        {
            var newnodes = new List<Node>();
            foreach (var node in merged)
            {
                foreach (var group in node.Nexts
                    .GroupBy(x => x.Name)
                    .Where(x => x.Count() >= 2)
                    .ToList())
                {
                    var merge = new Node { Name = group.Key, Lines = group.Select(x => x.Lines).Flatten().ToList() };
                    group.Each(x => x.Nexts.Each(y => merge.Nexts.Add(y)));
                    _ = node.Nexts.RemoveWhere(x => group.Contains(x));
                    _ = node.Nexts.Add(merge);
                    newnodes.Add(merge);
                }
            }
            if (newnodes.IsEmpty()) break;
            merged.AddRange(newnodes);
        }
        return merged.ToArray();
    }

    public static Node[] Sweep(Node start)
    {
        var marks = new List<Node>();
        void mark_proc(Node node)
        {
            if (marks.Contains(node)) return;
            marks.Add(node);
            node.Nexts.Each(mark_proc);
        };
        mark_proc(start);

        return marks.ToArray();
    }
}
