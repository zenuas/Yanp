using Parser;
using Xunit;
using Yanp;


public class LR0Next
{
    private Node[] RunString(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
        Yanp.SyntaxParser.ParseGrammar(syntax, lex);
        var nodes = LR0.CreateNodes(syntax);
        LR0.Next(nodes, LR0.First(nodes));
        return nodes;
    }

    [Fact]
    public void Next1()
    {
        var nodes = RunString("start : a a : 'A'");
        Assert.Equal(4, nodes.Length);
        Assert.Equal("start : . a; a : . 'A'", nodes[0].ToString());
        Assert.Equal("start : a .", nodes[1].ToString());
        Assert.Equal("a : . 'A'", nodes[2].ToString());
        Assert.Equal("a : 'A' .", nodes[3].ToString());
    }

    [Fact]
    public void Next2()
    {
        var nodes = RunString("start : a a : b b : c c : 'C'");
        Assert.Equal(8, nodes.Length);
        Assert.Equal("start : . a; a : . b; b : . c; c : . 'C'", nodes[0].ToString());
        Assert.Equal("start : a .", nodes[1].ToString());
        Assert.Equal("a : . b; b : . c; c : . 'C'", nodes[2].ToString());
        Assert.Equal("a : b .", nodes[3].ToString());
        Assert.Equal("b : . c; c : . 'C'", nodes[4].ToString());
        Assert.Equal("b : c .", nodes[5].ToString());
        Assert.Equal("c : . 'C'", nodes[6].ToString());
        Assert.Equal("c : 'C' .", nodes[7].ToString());
    }
}
