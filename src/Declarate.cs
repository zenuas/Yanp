namespace Yanp;

public class Declarate
{
    public required string Name { get; init; }
    public required AssocTypes Assoc { get; init; }
    public required int Priority { get; init; }
    public string Type { get; init; } = "";
    public bool IsTerminalSymbol { get; init; } = true;
    public bool IsAction { get; init; } = false;
}
