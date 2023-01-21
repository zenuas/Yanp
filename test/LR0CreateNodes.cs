using Parser;
using Xunit;
using Yanp;

public class LR0CreateNodes
{
    private Node[] RunString(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
        Yanp.SyntaxParser.ParseGrammar(syntax, lex);
        return LR0.CreateNodes(syntax);
    }

    [Fact]
    public void Null()
    {
        var nodes = RunString("");
        Assert.Empty(nodes);
    }

    [Fact]
    public void Node1()
    {
        var nodes = RunString("start :");
        _ = Assert.Single(nodes);
        _ = Assert.Single(nodes[0].Lines);
        Assert.Equal("start : .", nodes[0].Lines[0].ToString());
    }

    [Fact]
    public void Node2()
    {
        var nodes = RunString("start : a");
        Assert.Equal(2, nodes.Length);
        _ = Assert.Single(nodes[0].Lines);
        _ = Assert.Single(nodes[1].Lines);
        Assert.Equal("start : . a", nodes[0].Lines[0].ToString());
        Assert.Equal("start : a .", nodes[1].Lines[0].ToString());
    }

    [Fact]
    public void Node3()
    {
        var nodes = RunString("start : a b");
        Assert.Equal(3, nodes.Length);
        _ = Assert.Single(nodes[0].Lines);
        _ = Assert.Single(nodes[1].Lines);
        _ = Assert.Single(nodes[2].Lines);
        Assert.Equal("start : . a b", nodes[0].Lines[0].ToString());
        Assert.Equal("start : a . b", nodes[1].Lines[0].ToString());
        Assert.Equal("start : a b .", nodes[2].Lines[0].ToString());
    }
}
