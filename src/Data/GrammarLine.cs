using Extensions;
using Parser;
using System.Collections.Generic;
using System.Linq;

namespace Yanp.Data;

public class GrammarLine
{
    public required string Name { get; init; }
    public Token? Prec { get; init; }
    public Token? Action { get; init; }
    public List<Token> Grammars { get; init; } = new();

    public override string ToString()
    {
        return $"{Name} : {Grammars.Select(x => x.ToString()).Join(" ")}";
    }
}
