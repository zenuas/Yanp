using Mina.Extension;
using System.Collections.Generic;
using System.Linq;
using Yanp.Parser;

namespace Yanp.Data;

public class Node
{
    public required Token Name { get; init; }
    public List<GrammarLineIndex> Lines { get; init; } = [];
    public HashSet<Node> Nexts { get; } = [];
    public Dictionary<GrammarLineIndex, HashSet<Token>> Lookahead { get; } = [];

    public string[] LinesToString() => Lines.Select(x => $"{x}{(Lookahead.TryGetValue(x, out var value) && !value.IsEmpty() ? $" [{value.Select(x => x.ToString()).Order((a, b) => a.CompareTo(b)).Join(", ")}]" : "")}").ToArray();

    public override string ToString() => LinesToString().Join("; ");
}
