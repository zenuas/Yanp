using Parser;
using Xunit;
using Yanp.Data;

namespace Yanp.Test;

public class SyntaxParserParseGrammar
{
    private static Syntax RunString(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer { BaseReader = new() { BaseReader = new StringReader(text) } };
        SyntaxParser.ParseGrammar(syntax, lex);
        SyntaxParser.GrammarsToTerminalSymbol(syntax);
        return syntax;
    }

    private static Syntax RunString2(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer { BaseReader = new() { BaseReader = new StringReader(text) } };
        SyntaxParser.ParseDeclaration(syntax, lex);
        SyntaxParser.ParseGrammar(syntax, lex);
        SyntaxParser.GrammarsToTerminalSymbol(syntax);
        return syntax;
    }

    [Fact]
    public void Null()
    {
        var y = RunString("");
        Assert.Equal(y.Grammars.Count, 0);
    }

    [Fact]
    public void Gram1()
    {
        var y = RunString("a :");
        Assert.Equal(y.Start, "a");
        Assert.Equal(y.Grammars.Count, 1);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Equal(y.Grammars["a"].Count, 1);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Equal(yg.LineNumber, 1);
        Assert.Equal(yg.LineColumn, 1);
        Assert.Equal(yg.Grammars.Count, 0);
    }

    [Fact]
    public void Gram2()
    {
        var y = RunString("a : b 'c'");
        Assert.Equal(y.Start, "a");
        Assert.Equal(y.Grammars.Count, 1);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Equal(y.Grammars["a"].Count, 1);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Equal(yg.LineNumber, 1);
        Assert.Equal(yg.LineColumn, 1);
        Assert.Equal(yg.Grammars.Count, 2);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 7, Value = "'c'" });
    }

    [Fact]
    public void Gram3()
    {
        var y = RunString("a : b | c");
        Assert.Equal(y.Start, "a");
        Assert.Equal(y.Grammars.Count, 1);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Equal(y.Grammars["a"].Count, 2);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Equal(yg.LineNumber, 1);
        Assert.Equal(yg.LineColumn, 1);
        Assert.Equal(yg.Grammars.Count, 1);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        var yg2 = y.Grammars["a"][1];
        Assert.Null(yg2.Action);
        Assert.Null(yg2.Prec);
        Assert.Equal(yg2.LineNumber, 1);
        Assert.Equal(yg2.LineColumn, 7);
        Assert.Equal(yg2.Grammars.Count, 1);
        Assert.Equivalent(yg2.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 9, Value = "c" });
    }

    [Fact]
    public void Action1()
    {
        var y = RunString("a : {action}");
        Assert.Equal(y.Start, "a");
        Assert.Equal(y.Grammars.Count, 1);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Equal(y.Grammars["a"].Count, 1);
        var yg = y.Grammars["a"][0];
        Assert.True(yg.Action is { });
        Assert.Equivalent(yg.Action!, new Token() { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 5, Value = "action" });
        Assert.Null(yg.Prec);
        Assert.Equal(yg.LineNumber, 1);
        Assert.Equal(yg.LineColumn, 1);
        Assert.Equal(yg.Grammars.Count, 0);
    }

    [Fact]
    public void Action2()
    {
        var y = RunString("a : b b : 'B'");
        Assert.Equal(y.Start, "a");
        Assert.Equal(y.Grammars.Count, 2);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Equal(y.Grammars["a"].Count, 1);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Equal(yg.LineNumber, 1);
        Assert.Equal(yg.LineColumn, 1);
        Assert.Equal(yg.Grammars.Count, 1);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });

        Assert.True(y.Grammars.ContainsKey("b"));
        Assert.Equal(y.Grammars["b"].Count, 1);
        var yg2 = y.Grammars["b"][0];
        Assert.Null(yg2.Action);
        Assert.Null(yg2.Prec);
        Assert.Equal(yg2.LineNumber, 1);
        Assert.Equal(yg2.LineColumn, 7);
        Assert.Equal(yg2.Grammars.Count, 1);
        Assert.Equivalent(yg2.Grammars[0], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 11, Value = "'B'" });
    }

    [Fact]
    public void AnonymousAction1()
    {
        var y = RunString("a : b {anon_action} c {action}");
        Assert.Equal(y.Start, "a");
        Assert.Equal(y.Grammars.Count, 2);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Equal(y.Grammars["a"].Count, 1);
        var yg = y.Grammars["a"][0];
        Assert.True(yg.Action is { });
        Assert.Equivalent(yg.Action!, new Token() { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 23, Value = "action" });
        Assert.Null(yg.Prec);
        Assert.Equal(yg.LineNumber, 1);
        Assert.Equal(yg.LineColumn, 1);
        Assert.Equal(yg.Grammars.Count, 3);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 5, Value = "b" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "{1}" });
        Assert.Equivalent(yg.Grammars[2], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 21, Value = "c" });

        Assert.Equal(y.Grammars["{1}"].Count, 1);
        var yg2 = y.Grammars["{1}"][0];
        Assert.True(yg2.Action is { });
        Assert.Equivalent(yg2.Action!, new Token() { Type = Symbols.ACTION, LineNumber = 1, LineColumn = 7, Value = "anon_action" });
        Assert.Null(yg2.Prec);
        Assert.Equal(yg2.LineNumber, 1);
        Assert.Equal(yg2.LineColumn, 7);
        Assert.Equal(yg2.Grammars.Count, 0);
    }

    [Fact]
    public void Prec1()
    {
        var y = RunString2("%right p %% a : b %prec p");
        Assert.Equal(y.Start, "a");
        Assert.Equal(y.Grammars.Count, 1);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Equal(y.Grammars["a"].Count, 1);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Equal(yg.Priority, 1);
        Assert.Equal(AssocTypes.Right, yg.Assoc);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.PREC, LineNumber = 1, LineColumn = 25, Value = "p" });
        Assert.Equal(yg.LineNumber, 1);
        Assert.Equal(yg.LineColumn, 13);
        Assert.Equal(yg.Grammars.Count, 1);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 17, Value = "b" });
    }

    [Fact]
    public void Prec2()
    {
        var y = RunString2("%right p %% a : %prec p b");
        Assert.Equal(y.Start, "a");
        Assert.Equal(y.Grammars.Count, 1);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Equal(y.Grammars["a"].Count, 1);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Equal(yg.Priority, 1);
        Assert.Equal(AssocTypes.Right, yg.Assoc);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.PREC, LineNumber = 1, LineColumn = 23, Value = "p" });
        Assert.Equal(yg.LineNumber, 1);
        Assert.Equal(yg.LineColumn, 13);
        Assert.Equal(yg.Grammars.Count, 1);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 25, Value = "b" });
    }

    [Fact]
    public void Prec3()
    {
        var y = RunString2("%right p1 p2 %% a : %prec p1 b %prec p2 c");
        Assert.Equal(y.Start, "a");
        Assert.Equal(y.Grammars.Count, 1);
        Assert.True(y.Grammars.ContainsKey("a"));
        Assert.Equal(y.Grammars["a"].Count, 1);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Equal(yg.Priority, 1);
        Assert.Equal(AssocTypes.Right, yg.Assoc);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.PREC, LineNumber = 1, LineColumn = 27, Value = "p1" });
        Assert.Equal(yg.LineNumber, 1);
        Assert.Equal(yg.LineColumn, 17);
        Assert.Equal(yg.Grammars.Count, 2);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 30, Value = "b" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 41, Value = "c" });
    }

    [Fact]
    public void Expr1()
    {
        var y = RunString2("%left '+' %% expr : NUM '+' NUM");
        Assert.Equal(y.Start, "expr");
        Assert.Equal(y.Grammars.Count, 1);
        Assert.Equal(y.Grammars["expr"].Count, 1);
        var yg = y.Grammars["expr"][0];
        Assert.Equal(yg.Priority, 1);
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
        Assert.Equal(y.Start, "expr");
        Assert.Equal(y.Grammars.Count, 1);
        Assert.Equal(y.Grammars["expr"].Count, 2);
        var yg = y.Grammars["expr"][0];
        Assert.Equal(yg.Priority, 1);
        Assert.Equal(AssocTypes.Left, yg.Assoc);
        Assert.Equivalent(yg.Prec, new Token { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 35, Value = "'+'" });
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 31, Value = "NUM" });
        Assert.Equivalent(yg.Grammars[1], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 35, Value = "'+'" });
        Assert.Equivalent(yg.Grammars[2], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 39, Value = "NUM" });

        var yg2 = y.Grammars["expr"][1];
        Assert.Equal(yg2.Priority, 2);
        Assert.Equal(AssocTypes.Left, yg2.Assoc);
        Assert.Equivalent(yg2.Prec, new Token { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 49, Value = "'*'" });
        Assert.Equivalent(yg2.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 45, Value = "NUM" });
        Assert.Equivalent(yg2.Grammars[1], new Token() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 49, Value = "'*'" });
        Assert.Equivalent(yg2.Grammars[2], new Token() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 53, Value = "NUM" });
    }
}
