using Parser;

namespace Yanp;

public class ParserGenerator
{
    public static void ParseDeclaration(Syntax syntax, Lexer lex)
    {
        while (lex.PeekToken().Type != Symbols.__EOF)
        {
            lex.ReadToken();
        }
    }

    public static void ParseGrammar(Lexer lex)
    {
    }
}
