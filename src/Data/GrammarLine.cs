using Mina.Extension;
using System.Collections.Generic;
using System.Linq;
using Yanp.Parser;

namespace Yanp.Data;

public class GrammarLine
{
    public required Token Name { get; init; }
    public required int LineNumber { get; init; }
    public required int LineColumn { get; init; }
    public Token? Prec { get; init; }
    public Token? Action { get; init; }
    public List<Token> Grammars { get; init; } = [];
    public int Priority { get; init; } = 0;
    public AssocTypes Assoc { get; init; } = AssocTypes.Type;

    public override string ToString() => $"{Name} : {Grammars.Select(x => x.ToString()).Join(" ")}";
}
