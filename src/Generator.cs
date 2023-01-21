using Parser;
using System.Collections.Generic;
using System.Linq;

namespace Yanp;

public class Generator
{
    public static void LR0(Syntax syntax)
    {
        var accept = new Token { Type = Symbols.VAR, LineNumber = 0, LineColumn = 0, Value = "$ACCEPT" };
        var end = new Token { Type = Symbols.VAR, LineNumber = 0, LineColumn = 0, Value = "$END" };

        syntax.Declares.Add(accept.Value, new Declarate { Name = accept.Value, Assoc = AssocTypes.Type, IsTerminalSymbol = false });
        syntax.Declares.Add(end.Value, new Declarate { Name = end.Value, Assoc = AssocTypes.Type });

        // $ACCEPT : syntax.Start $END
        syntax.Grammars.Add(accept.Value, new() {
            new() {
                Grammars = new() {
                    new() { Type = Symbols.VAR, LineNumber = 0, LineColumn = 0, Value = syntax.Start },
                    end
                }
            }
        });

        // first
        var first = syntax.Grammars.ToDictionary(x => x.Key, x => new List<Node>());
    }
}
