using Extensions;
using Parser;
using System.Linq;

namespace Yanp;

public class LR0
{
    public static void Generate(Syntax syntax)
    {
        AddAccept(syntax);
        var nodes = CreateNodes(syntax);
        var first = First(nodes);
    }

    public static void AddAccept(Syntax syntax)
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
    }

    public static Node[] CreateNodes(Syntax syntax)
    {
        return syntax.Grammars.Select(g =>
            g.Value.Select(gl =>
                {
                    var line = Lists.Sequence(0)
                        .Select(i => new Node { Name = i == 0 ? g.Key : gl.Grammars[i - 1].Value, Lines = new() { new() { Name = g.Key, Index = i, Line = gl } } })
                        .Take(gl.Grammars.Count + 1)
                        .ToArray();

                    line.Take(gl.Grammars.Count).Each((x, i) => x.Nexts.Add(line[i + 1]));
                    return line;
                }))
            .Flatten()
            .Flatten()
            .ToArray();
    }

    public static Node[] First(Node[] nodes)
    {
        return nodes
            .Where(x => x.Lines[0].Index == 0)
            .ToArray();
    }
}
