using Extensions;
using System.Linq;

namespace Yanp.Data;

public class GrammarLineIndex
{
    public required GrammarLine Line { get; init; }
    public required int Index { get; init; }

    public override string ToString()
    {
        var line = Line.Grammars.Select(x => x.ToString()).ToList();
        line.Insert(Index, ".");

        return $"{Line.Name} : {line.Join(" ")}";
    }
}
