using Parser;
using System.Collections.Generic;

namespace Yanp;

public class GrammarLine
{
    public Token? Prec { get; init; }
    public Token? Action { get; init; }
    public List<Token> Grammars { get; init; } = new();
}
