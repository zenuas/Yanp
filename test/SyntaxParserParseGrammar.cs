using Parser;
using Xunit;
using Yanp.Data;

namespace Yanp.Test;

public class SyntaxParserParseGrammar
{
    private static Syntax RunString(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
        SyntaxParser.ParseGrammar(syntax, lex);
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
    }

    [Fact]
    public void Gram2()
    {
        var y = RunString("a : b 'c'");
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Equal(2, yg.Grammars.Count);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 7, Value = "'c" });
    }

    [Fact]
    public void Gram3()
    {
        var y = RunString("a : b | c");
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Equal(2, y.Grammars["a"].Count);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Single(yg.Grammars);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        var yg2 = y.Grammars["a"][1];
        Assert.Null(yg2.Action);
        Assert.Null(yg2.Prec);
        Assert.Single(yg2.Grammars);
        Assert.Equivalent(yg2.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 9, Value = "c" });
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

    [Fact]
    public void Prec1()
    {
        var y = RunString("a : b %prec p");
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.PREC, LineNumber = 1, LineColumn = 13, Value = "p" });
        _ = Assert.Single(yg.Grammars);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
    }

    [Fact]
    public void Prec2()
    {
        var y = RunString("a : %prec p b");
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.PREC, LineNumber = 1, LineColumn = 11, Value = "p" });
        _ = Assert.Single(yg.Grammars);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 13, Value = "b" });
    }

    [Fact]
    public void Prec3()
    {
        var y = RunString("a : %prec p1 b %prec p2 c");
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.PREC, LineNumber = 1, LineColumn = 11, Value = "p1" });
        Assert.Equal(2, yg.Grammars.Count);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 14, Value = "b" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 25, Value = "c" });
    }
}
