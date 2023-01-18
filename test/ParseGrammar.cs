using Parser;
using Xunit;
using Yanp;
using static ParserGrammarLines;

public class ParseGrammar
{
    private static Syntax RunString(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer(new SourceCodeReader() { BaseReader = new StringReader(text) });
        ParserGenerator.ParseGrammar(syntax, lex);
        return syntax;
    }

    [Fact]
    public void Null()
    {
        var y = RunString("");
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void Gram1()
    {
        var y = RunString("a :");
        Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Empty(yg.Grammars);

        var y2 = RunString("a : b 'c'");
        Assert.Single(y2.Grammars);
        Assert.True(y2.Grammars.ContainsKey("a"));
        Assert.Single(y2.Grammars["a"]);
        var yg2 = y2.Grammars["a"][0];
        Assert.Null(yg2.Action);
        Assert.Null(yg2.Prec);
        Assert.Equal(2, yg2.Grammars.Count);
        Assert.Equal(yg2.Grammars[0], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" }, new EqToken());
        Assert.Equal(yg2.Grammars[1], new Token { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 7, Value = "'c" }, new EqToken());
    }

    [Fact]
    public void Action1()
    {
        var y = RunString("a : {action}");
        Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.True(yg.Action is { });
        Assert.Equal(yg.Action!, new Token { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 5, Value = "action" }, new EqToken());
        Assert.Null(yg.Prec);
        Assert.Empty(yg.Grammars);
    }

    [Fact]
    public void Action2()
    {
        var y = RunString("a : b b : 'B'");
        Assert.Equal(2, y.Grammars.Count);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Single(yg.Grammars);
        Assert.Equal(yg.Grammars[0], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" }, new EqToken());

        Assert.True(y.Grammars.ContainsKey("b"));
        Assert.Single(y.Grammars["b"]);
        var yg2 = y.Grammars["b"][0];
        Assert.Null(yg2.Action);
        Assert.Null(yg2.Prec);
        Assert.Single(yg2.Grammars);
        Assert.Equal(yg2.Grammars[0], new Token { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 11, Value = "'B" }, new EqToken());
    }

    [Fact]
    public void AnonymousAction1()
    {
        var y = RunString("a : b {anon_action} c {action}");
        Assert.Equal(2, y.Grammars.Count);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.True(yg.Action is { });
        Assert.Equal(yg.Action!, new Token { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 23, Value = "action" }, new EqToken());
        Assert.Null(yg.Prec);
        Assert.Equal(3, yg.Grammars.Count);
        Assert.Equal(yg.Grammars[0], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" }, new EqToken());
        Assert.Equal(yg.Grammars[1], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "{1}" }, new EqToken());
        Assert.Equal(yg.Grammars[2], new Token { Type = Symbols.VAR, LineNumber = 1, LineColumn = 21, Value = "c" }, new EqToken());

        Assert.Single(y.Grammars["{1}"]);
        var yg2 = y.Grammars["{1}"][0];
        Assert.True(yg2.Action is { });
        Assert.Equal(yg2.Action!, new Token { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 7, Value = "anon_action" }, new EqToken());
        Assert.Null(yg2.Prec);
        Assert.Empty(yg2.Grammars);
    }
}
