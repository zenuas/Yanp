using Parser;
using Xunit;
using Yanp.Data;

namespace Yanp.Test;

public class LALR1Nullable
{
    private static HashSet<string> RunString(string text, string start = "start")
    {
        var syntax = new Syntax();
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
        Yanp.SyntaxParser.ParseGrammar(syntax, lex);
        syntax.Start = start;
        var nodes = LR0.Generate(syntax);
        var lines = LALR1.GrammarLines(nodes);
        return LALR1.Nullable(nodes, lines);
    }

    [Fact]
    public void Nullable1()
    {
        var hash = RunString("start :");
        _ = Assert.Single(hash);
        Assert.Contains("start", hash);
    }

    [Fact]
    public void Nullable2()
    {
        var hash = RunString("start : a 'B' a :");
        _ = Assert.Single(hash);
        Assert.Contains("a", hash);
    }

    [Fact]
    public void Nullable3()
    {
        var hash = RunString("start : a b 'C' a : void b : void void :");
        Assert.Equal(3, hash.Count);
        Assert.Contains("a", hash);
        Assert.Contains("b", hash);
        Assert.Contains("void", hash);
    }
}
