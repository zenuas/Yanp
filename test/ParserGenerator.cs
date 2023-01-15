using Extensions;
using Parser;
using Xunit;

public class ParserGenerator
{
    private static Yanp.Syntax RunString(string text)
    {
        var syntax = new Yanp.Syntax();
        var lex = new Lexer(new SourceCodeReader() { BaseReader = new StringReader(text) });
        Yanp.ParserGenerator.ParseDeclaration(syntax, lex);
        return syntax;
    }

    [Fact]
    public void ParseDeclaration()
    {
        RunString("");
    }
}
