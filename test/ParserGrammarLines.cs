using Parser;
using Xunit;
using Yanp;

public class ParserGrammarLines
{
    public class EqToken : IEqualityComparer<Token>
    {
        public bool Equals(Token? x, Token? y)
        {
            return x?.Type == y?.Type &&
                x?.LineNumber == y?.LineNumber &&
                x?.LineColumn == y?.LineColumn &&
                x?.Value == y?.Value;
        }

        public int GetHashCode(Token obj) => obj.GetHashCode();
    }

    private static Token[][] RunString(string text)
    {
        var lex = new Lexer(new SourceCodeReader() { BaseReader = new StringReader(text) });
        return ParserGenerator.ParserGrammarLines(lex).Select(x =>
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
        Assert.Single(tss);
        Assert.Single(tss[0]);
        Assert.Equal(tss[0][0], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 1, Value = "a" }, new EqToken());

        var tss1 = RunString("a : b");
        Assert.Single(tss1);
        Assert.Equal(2, tss1[0].Length);
        Assert.Equal(tss1[0][0], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 1, Value = "a" }, new EqToken());
        Assert.Equal(tss1[0][1], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" }, new EqToken());

        var tss2 = RunString("a : b c : d");
        Assert.Equal(2, tss2.Length);
        Assert.Equal(2, tss2[0].Length);
        Assert.Equal(tss2[0][0], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 1, Value = "a" }, new EqToken());
        Assert.Equal(tss2[0][1], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" }, new EqToken());
        Assert.Equal(2, tss2[1].Length);
        Assert.Equal(tss2[1][0], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "c" }, new EqToken());
        Assert.Equal(tss2[1][1], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 11, Value = "d" }, new EqToken());
    }
}
