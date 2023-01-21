using Parser;
using Xunit;

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

        var tss1 = RunString("a : b");
        _ = Assert.Single(tss1);
        Assert.Equal(2, tss1[0].Length);
        Assert.Equivalent(tss1[0][0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 1, Value = "a" });
        Assert.Equivalent(tss1[0][1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });

        var tss2 = RunString("a : b c : d");
        Assert.Equal(2, tss2.Length);
        Assert.Equal(2, tss2[0].Length);
        Assert.Equivalent(tss2[0][0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 1, Value = "a" });
        Assert.Equivalent(tss2[0][1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        Assert.Equal(2, tss2[1].Length);
        Assert.Equivalent(tss2[1][0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "c" });
        Assert.Equivalent(tss2[1][1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 11, Value = "d" });
    }
}
