using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Yanp;

public class Node
{
    public required string Name { get; init; }
    public List<GrammarLineIndex> Lines { get; init; } = new();
    public HashSet<Node> Nexts { get; } = new();

    public override string ToString() => Lines.Select(x => x.ToString()).Join("; ");
}
