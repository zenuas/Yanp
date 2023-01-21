using Command;
using Extensions;
using System.IO;

namespace Yanp;

public class Program
{
    public static void Main(string[] args)
    {
        var opt = new Option();
        var xs = CommandLine.Run(opt, args);

        if (xs.Length == 0)
        {
            Run(opt.Input);
        }
        else
        {
            xs.MapParallel(x => new StreamReader(x)).Each(async x => Run(await x));
        }
    }

    public static void Run(TextReader input)
    {
        var syntax = SyntaxParser.Parse(input);

        if (syntax.Grammars.IsEmpty()) throw new ParseException("no grammar has been specified");
        if (!syntax.Grammars.Contains(x => x.Key == syntax.Start)) throw new ParseException("no grammar has been specified");

        LR0.Generate(syntax);
    }
}
