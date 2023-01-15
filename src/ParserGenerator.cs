using Parser;
using System.Collections.Generic;

namespace Yanp;

public class ParserGenerator
{
    public static void ParseDeclaration(Syntax syntax, Lexer lex)
    {
        var assoc_conv = new Dictionary<Symbols, AssocTypes>() {
            { Symbols.TOKEN, AssocTypes.Type },
            { Symbols.LEFT, AssocTypes.Left },
            { Symbols.RIGHT, AssocTypes.Right },
            { Symbols.NONASSOC, AssocTypes.Nonassoc },
            { Symbols.TYPE, AssocTypes.Type },
        };
        var priority = 0;

        var read_assoc = (AssocTypes assoc, int pri) =>
        {
            if (lex.PeekToken().Type == Symbols.__EOF) throw new SyntaxErrorException($"assoc {assoc} with token") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };

            var type = lex.PeekToken().Type == Symbols.DECLARE ? lex.ReadToken().Value : "";

            if (lex.PeekToken().Type != Symbols.VAR && lex.PeekToken().Type != Symbols.CHAR) throw new SyntaxErrorException($"assoc {assoc} with token") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };
            do
            {
                var t = lex.ReadToken();
                syntax.Declares.Add(t.Value, new Declarate { Assoc = assoc, Name = t.Value, Priority = pri });
            } while (lex.PeekToken().Type == Symbols.VAR || lex.PeekToken().Type == Symbols.CHAR);
        };

        while (lex.PeekToken().Type != Symbols.__EOF)
        {
            var t = lex.ReadToken();
            switch (t.Type)
            {
                case Symbols.TOKEN:
                case Symbols.LEFT:
                case Symbols.RIGHT:
                case Symbols.NONASSOC:
                case Symbols.TYPE:
                    if (t.Type == Symbols.TYPE && lex.PeekToken().Type != Symbols.DECLARE) throw new SyntaxErrorException($"assoc {t.Type} with <decla-type>") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };
                    read_assoc(assoc_conv[t.Type], ++priority);
                    break;

                case Symbols.PartEnd:
                    return;
            }
        }
    }

    public static void ParseGrammar(Lexer lex)
    {
    }
}
