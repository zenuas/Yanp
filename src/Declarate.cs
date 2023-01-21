using System.Collections.Generic;

namespace Yanp;

public class Declarate
{
    public required string Name { get; init; }
    public required AssocTypes Assoc { get; init; }
    public int Priority { get; init; } = 0;
    public string Type { get; init; } = "";
    public bool IsTerminalSymbol { get; init; } = true;
    public bool IsAction { get; init; } = false;

    public override string ToString()
    {
        var assoc_name = new Dictionary<AssocTypes, string>() {
            { AssocTypes.Type, "%type" },
            { AssocTypes.Left, "%left" },
            { AssocTypes.Right, "%right" },
            { AssocTypes.Nonassoc, "%nonassoc" },
        };

        return $"{assoc_name[Assoc]}{(Type == "" ? "" : $"<{Type}>")} {Name}";
    }
}
