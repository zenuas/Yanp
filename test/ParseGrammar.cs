using Parser;
using Xunit;
using Yanp;
using static ParserGrammarLines;

public class ParseGrammar
{
    private static Syntax RunString(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
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
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Empty(yg.Grammars);

        var y2 = RunString("a : b 'c'");
        _ = Assert.Single(y2.Grammars);
        Assert.True(y2.Grammars.ContainsKey("a"));
        _ = Assert.Single(y2.Grammars["a"]);
        var yg2 = y2.Grammars["a"][0];
        Assert.Null(yg2.Action);
        Assert.Null(yg2.Prec);
        Assert.Equal(2, yg2.Grammars.Count);
        Assert.Equivalent(yg2.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        Assert.Equivalent(yg2.Grammars[1], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 7, Value = "'c" });
    }

    [Fact]
    public void Action1()
    {
        var y = RunString("a : {action}");
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.True(yg.Action is { });
        Assert.Equivalent(yg.Action!, new Token() { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 5, Value = "action" });
        Assert.Null(yg.Prec);
        Assert.Empty(yg.Grammars);
    }

    [Fact]
    public void Action2()
    {
        var y = RunString("a : b b : 'B'");
        Assert.Equal(2, y.Grammars.Count);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        _ = Assert.Single(yg.Grammars);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });

        Assert.True(y.Grammars.ContainsKey("b"));
        _ = Assert.Single(y.Grammars["b"]);
        var yg2 = y.Grammars["b"][0];
        Assert.Null(yg2.Action);
        Assert.Null(yg2.Prec);
        _ = Assert.Single(yg2.Grammars);
        Assert.Equivalent(yg2.Grammars[0], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 11, Value = "'B" });
    }

    [Fact]
    public void AnonymousAction1()
    {
        var y = RunString("a : b {anon_action} c {action}");
        Assert.Equal(2, y.Grammars.Count);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.True(yg.Action is { });
        Assert.Equivalent(yg.Action!, new Token() { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 23, Value = "action" });
        Assert.Null(yg.Prec);
        Assert.Equal(3, yg.Grammars.Count);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "{1}" });
        Assert.Equivalent(yg.Grammars[2], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 21, Value = "c" });

        _ = Assert.Single(y.Grammars["{1}"]);
        var yg2 = y.Grammars["{1}"][0];
        Assert.True(yg2.Action is { });
        Assert.Equivalent(yg2.Action!, new Token() { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 7, Value = "anon_action" });
        Assert.Null(yg2.Prec);
        Assert.Empty(yg2.Grammars);
    }
}
