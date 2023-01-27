using Extensions;
using Parser;
using System.Collections.Generic;
using System.Linq;

namespace Yanp.Data;

public class Node
{
    public required Token Name { get; init; }
    public List<GrammarLineIndex> Lines { get; init; } = new();
    public HashSet<Node> Nexts { get; } = new();

    public override string ToString() => Lines.Select(x => x.ToString()).Join("; ");
}
