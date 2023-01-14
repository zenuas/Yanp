using Command;
using Extensions;
using Parser;
using System.IO;
using static ParserGenerator;

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

void Run(TextReader input)
{
    var lex = new Lexer(new SourceCodeReader() { BaseReader = input });
    ParseDeclaration(lex);
    ParseGrammar(lex);
}

