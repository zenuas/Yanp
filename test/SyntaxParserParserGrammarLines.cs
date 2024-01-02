using Xunit;
using Yanp.Parser;

namespace Yanp.Test;

public class SyntaxParserParserGrammarLines
{
    private static Token[][] RunString(string text)
    {
        var lex = new Lexer { BaseReader = new() { BaseReader = new StringReader(text) } };
        return SyntaxParser.ParserGrammarLines(lex).Select(x =>
        {
            var t = new List<Token> { x.Head };
            t.AddRange(x.Grammars);
            return t.ToArray();
        }).ToArray();
    }

    [Fact]
    public void Gram1()
    {
        var tss = RunString("a :");
        Assert.Equal(tss.Length, 1);
        Assert.Equal(tss[0].Length, 1);
        Assert.Equivalent(tss[0][0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 1, Value = "a" });
    }

    [Fact]
    public void Gram2()
    {
        var tss = RunString("a : b");
        Assert.Equal(tss.Length, 1);
        Assert.Equal(tss[0].Length, 2);
        Assert.Equivalent(tss[0][0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 1, Value = "a" });
        Assert.Equivalent(tss[0][1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
    }

    [Fact]
    public void Gram3()
    {
        var tss = RunString("a : b c : d");
        Assert.Equal(tss.Length, 2);
        Assert.Equal(tss[0].Length, 2);
        Assert.Equivalent(tss[0][0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 1, Value = "a" });
        Assert.Equivalent(tss[0][1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        Assert.Equal(tss[1].Length, 2);
        Assert.Equivalent(tss[1][0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "c" });
        Assert.Equivalent(tss[1][1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 11, Value = "d" });
    }
}
