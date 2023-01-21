using System.Collections.Generic;

namespace Yanp;

public class Node
{
    public required string Name { get; init; }
    public List<GrammarLineIndex> Lines { get; init; } = new();
    public List<Node> Nexts { get; } = new();
}
