namespace Yanp.Parser;

public class Token
{
    public required Symbols Type { get; init; }
    public required int LineNumber { get; init; }
    public required int LineColumn { get; init; }
    public string Value { get; init; } = "";
    public override string ToString() => Value;
}
