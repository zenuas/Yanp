namespace Yanp.Data;

public class ReduceAction : IParserAction
{
    public required GrammarLine Reduce { get; init; }
}
