using Parser;
using System.Collections.Generic;

namespace Yanp.Data;

public class Table
{
    public required int Index { get; init; }
    public required Node Node { get; init; }
    public bool AnyReduce { get; init; } = false;
    public string[] Conflicts { get; init; } = new string[0];
    public Dictionary<Token, IParserAction> Actions { get; init; } = new();
}
