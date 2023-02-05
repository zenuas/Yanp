namespace Yanp.Data;

public class ShiftAction : IParserAction
{
    public required Node Next { get; init; }
    public required int Priority { get; init; }
}
