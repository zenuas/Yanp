using Parser;
using Xunit;

namespace Yanp.Test;

public class ParserGrammarLines
{
    private static Token[][] RunString(string text)
    {
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
        return Yanp.SyntaxParser.ParserGrammarLines(lex).Select(x =>
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
        _ = Assert.Single(tss);
        _ = Assert.Single(tss[0]);
        Assert.Equivalent(tss[0][0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 1, Value = "a" });
    }

    [Fact]
    public void Gram2()
    {
        var tss = RunString("a : b");
        _ = Assert.Single(tss);
        Assert.Equal(2, tss[0].Length);
        Assert.Equivalent(tss[0][0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 1, Value = "a" });
        Assert.Equivalent(tss[0][1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
    }

    [Fact]
    public void Gram3()
    {
        var tss = RunString("a : b c : d");
        Assert.Equal(2, tss.Length);
        Assert.Equal(2, tss[0].Length);
        Assert.Equivalent(tss[0][0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 1, Value = "a" });
        Assert.Equivalent(tss[0][1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        Assert.Equal(2, tss[1].Length);
        Assert.Equivalent(tss[1][0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "c" });
        Assert.Equivalent(tss[1][1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 11, Value = "d" });
    }
}
