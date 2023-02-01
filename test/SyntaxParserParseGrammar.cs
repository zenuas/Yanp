﻿using Parser;
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

    private static Syntax RunString2(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
        SyntaxParser.ParseDeclaration(syntax, lex);
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
        Assert.Equal("a", y.Start);
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Equal(1, yg.LineNumber);
        Assert.Equal(1, yg.LineColumn);
        Assert.Empty(yg.Grammars);
    }

    [Fact]
    public void Gram2()
    {
        var y = RunString("a : b 'c'");
        Assert.Equal("a", y.Start);
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Equal(1, yg.LineNumber);
        Assert.Equal(1, yg.LineColumn);
        Assert.Equal(2, yg.Grammars.Count);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 7, Value = "'c'" });
    }

    [Fact]
    public void Gram3()
    {
        var y = RunString("a : b | c");
        Assert.Equal("a", y.Start);
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Equal(2, y.Grammars["a"].Count);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Equal(1, yg.LineNumber);
        Assert.Equal(1, yg.LineColumn);
        _ = Assert.Single(yg.Grammars);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        var yg2 = y.Grammars["a"][1];
        Assert.Null(yg2.Action);
        Assert.Null(yg2.Prec);
        Assert.Equal(1, yg2.LineNumber);
        Assert.Equal(7, yg2.LineColumn);
        _ = Assert.Single(yg2.Grammars);
        Assert.Equivalent(yg2.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 9, Value = "c" });
    }

    [Fact]
    public void Action1()
    {
        var y = RunString("a : {action}");
        Assert.Equal("a", y.Start);
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.True(yg.Action is { });
        Assert.Equivalent(yg.Action!, new Token() { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 5, Value = "action" });
        Assert.Null(yg.Prec);
        Assert.Equal(1, yg.LineNumber);
        Assert.Equal(1, yg.LineColumn);
        Assert.Empty(yg.Grammars);
    }

    [Fact]
    public void Action2()
    {
        var y = RunString("a : b b : 'B'");
        Assert.Equal("a", y.Start);
        Assert.Equal(2, y.Grammars.Count);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Equal(1, yg.LineNumber);
        Assert.Equal(1, yg.LineColumn);
        _ = Assert.Single(yg.Grammars);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });

        Assert.True(y.Grammars.ContainsKey("b"));
        _ = Assert.Single(y.Grammars["b"]);
        var yg2 = y.Grammars["b"][0];
        Assert.Null(yg2.Action);
        Assert.Null(yg2.Prec);
        Assert.Equal(1, yg2.LineNumber);
        Assert.Equal(7, yg2.LineColumn);
        _ = Assert.Single(yg2.Grammars);
        Assert.Equivalent(yg2.Grammars[0], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 11, Value = "'B'" });
    }

    [Fact]
    public void AnonymousAction1()
    {
        var y = RunString("a : b {anon_action} c {action}");
        Assert.Equal("a", y.Start);
        Assert.Equal(2, y.Grammars.Count);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.True(yg.Action is { });
        Assert.Equivalent(yg.Action!, new Token() { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 23, Value = "action" });
        Assert.Null(yg.Prec);
        Assert.Equal(1, yg.LineNumber);
        Assert.Equal(1, yg.LineColumn);
        Assert.Equal(3, yg.Grammars.Count);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "{1}" });
        Assert.Equivalent(yg.Grammars[2], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 21, Value = "c" });

        _ = Assert.Single(y.Grammars["{1}"]);
        var yg2 = y.Grammars["{1}"][0];
        Assert.True(yg2.Action is { });
        Assert.Equivalent(yg2.Action!, new Token() { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 7, Value = "anon_action" });
        Assert.Null(yg2.Prec);
        Assert.Equal(1, yg2.LineNumber);
        Assert.Equal(7, yg2.LineColumn);
        Assert.Empty(yg2.Grammars);
    }

    [Fact]
    public void Prec1()
    {
        var y = RunString2("%right p %% a : b %prec p");
        Assert.Equal("a", y.Start);
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Equal(1, yg.Priority);
        Assert.Equal(AssocTypes.Right, yg.Assoc);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.PREC, LineNumber = 1, LineColumn = 25, Value = "p" });
        Assert.Equal(1, yg.LineNumber);
        Assert.Equal(13, yg.LineColumn);
        _ = Assert.Single(yg.Grammars);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 17, Value = "b" });
    }

    [Fact]
    public void Prec2()
    {
        var y = RunString2("%right p %% a : %prec p b");
        Assert.Equal("a", y.Start);
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Equal(1, yg.Priority);
        Assert.Equal(AssocTypes.Right, yg.Assoc);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.PREC, LineNumber = 1, LineColumn = 23, Value = "p" });
        Assert.Equal(1, yg.LineNumber);
        Assert.Equal(13, yg.LineColumn);
        _ = Assert.Single(yg.Grammars);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 25, Value = "b" });
    }

    [Fact]
    public void Prec3()
    {
        var y = RunString2("%right p1 p2 %% a : %prec p1 b %prec p2 c");
        Assert.Equal("a", y.Start);
        _ = Assert.Single(y.Grammars);
        Assert.True(y.Grammars.ContainsKey("a"));
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Equal(1, yg.Priority);
        Assert.Equal(AssocTypes.Right, yg.Assoc);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.PREC, LineNumber = 1, LineColumn = 27, Value = "p1" });
        Assert.Equal(1, yg.LineNumber);
        Assert.Equal(17, yg.LineColumn);
        Assert.Equal(2, yg.Grammars.Count);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 30, Value = "b" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 41, Value = "c" });
    }

    [Fact]
    public void Expr1()
    {
        var y = RunString2("%left '+' %% expr : NUM '+' NUM");
        Assert.Equal("expr", y.Start);
        _ = Assert.Single(y.Grammars);
        _ = Assert.Single(y.Grammars["expr"]);
        var yg = y.Grammars["expr"][0];
        Assert.Equal(1, yg.Priority);
        Assert.Equal(AssocTypes.Left, yg.Assoc);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 25, Value = "'+'" });
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 21, Value = "NUM" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 25, Value = "'+'" });
        Assert.Equivalent(yg.Grammars[2], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 29, Value = "NUM" });
    }

    [Fact]
    public void Expr2()
    {
        var y = RunString2("%left '+' %left '*' %% expr : NUM '+' NUM | NUM '*' NUM");
        Assert.Equal("expr", y.Start);
        _ = Assert.Single(y.Grammars);
        Assert.Equal(2, y.Grammars["expr"].Count);
        var yg = y.Grammars["expr"][0];
        Assert.Equal(1, yg.Priority);
        Assert.Equal(AssocTypes.Left, yg.Assoc);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 35, Value = "'+'" });
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 31, Value = "NUM" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 35, Value = "'+'" });
        Assert.Equivalent(yg.Grammars[2], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 39, Value = "NUM" });

        var yg2 = y.Grammars["expr"][1];
        Assert.Equal(2, yg2.Priority);
        Assert.Equal(AssocTypes.Left, yg2.Assoc);
        Assert.Equivalent(yg2.Prec, new Token { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 49, Value = "'*'" });
        Assert.Equivalent(yg2.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 45, Value = "NUM" });
        Assert.Equivalent(yg2.Grammars[1], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 49, Value = "'*'" });
        Assert.Equivalent(yg2.Grammars[2], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 53, Value = "NUM" });
    }
}
