using Parser;
using Xunit;
using Yanp.Data;

namespace Yanp.Test;

public class LR0Next
{
    private static Node[] RunString(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer { BaseReader = new() { BaseReader = new StringReader(text) } };
        SyntaxParser.ParseGrammar(syntax, lex);
        SyntaxParser.GrammarsToTerminalSymbol(syntax);
        var nodes = LR0.CreateNodes(syntax);
        LR0.Next(nodes, LR0.First(nodes));
        return nodes;
    }

    [Fact]
    public void Next1()
    {
        var nodes = RunString("start : a a : 'A'");
        Assert.Equal(nodes.Length, 4);
        Assert.Equal(nodes[0].ToString(), "start : . a; a : . 'A'");
        Assert.Equal(nodes[1].ToString(), "start : a .");
        Assert.Equal(nodes[2].ToString(), "a : . 'A'");
        Assert.Equal(nodes[3].ToString(), "a : 'A' .");
    }

    [Fact]
    public void Next2()
    {
        var nodes = RunString("start : a a : b b : c c : 'C'");
        Assert.Equal(nodes.Length, 8);
        Assert.Equal(nodes[0].ToString(), "start : . a; a : . b; b : . c; c : . 'C'");
        Assert.Equal(nodes[1].ToString(), "start : a .");
        Assert.Equal(nodes[2].ToString(), "a : . b; b : . c; c : . 'C'");
        Assert.Equal(nodes[3].ToString(), "a : b .");
        Assert.Equal(nodes[4].ToString(), "b : . c; c : . 'C'");
        Assert.Equal(nodes[5].ToString(), "b : c .");
        Assert.Equal(nodes[6].ToString(), "c : . 'C'");
        Assert.Equal(nodes[7].ToString(), "c : 'C' .");
    }
}
