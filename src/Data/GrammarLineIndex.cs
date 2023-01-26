using Extensions;
using Parser;
using System.Collections.Generic;
using System.Linq;

namespace Yanp.Data;

public class GrammarLineIndex
{
    public required GrammarLine Line { get; init; }
    public required int Index { get; init; }
    public HashSet<Token> Lookahead { get; } = new();

    public override string ToString()
    {
        var line = Line.Grammars.Select(x => x.ToString()).ToList();
        line.Insert(Index, ".");

        var lookahead = Lookahead
            .Select(x => x.ToString())
            .Sort((a, b) => a.CompareTo(b))
            .Join(", ");

        return $"{Line.Name} : {line.Join(" ")}{(lookahead == "" ? "" : $" [{lookahead}]")}";
    }
}
