using Parser;
using Xunit;
using Yanp.Data;

namespace Yanp.Test;

public class LR0CreateNodes
{
    private static Node[] RunString(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer { BaseReader = new() { BaseReader = new StringReader(text) } };
        SyntaxParser.ParseGrammar(syntax, lex);
        SyntaxParser.GrammarsToTerminalSymbol(syntax);
        return LR0.CreateNodes(syntax);
    }

    [Fact]
    public void Null()
    {
        var nodes = RunString("");
        Assert.Equal(nodes.Length, 0);
    }

    [Fact]
    public void Node1()
    {
        var nodes = RunString("start :");
        Assert.Equal(nodes.Length, 1);
        Assert.Equal(nodes[0].Lines.Count, 1);
        Assert.Equal(nodes[0].Lines[0].ToString(), "start : .");
    }

    [Fact]
    public void Node2()
    {
        var nodes = RunString("start : a");
        Assert.Equal(nodes.Length, 2);
        Assert.Equal(nodes[0].Lines.Count, 1);
        Assert.Equal(nodes[1].Lines.Count, 1);
        Assert.Equal(nodes[0].Lines[0].ToString(), "start : . a");
        Assert.Equal(nodes[1].Lines[0].ToString(), "start : a .");
    }

    [Fact]
    public void Node3()
    {
        var nodes = RunString("start : a b");
        Assert.Equal(nodes.Length, 3);
        Assert.Equal(nodes[0].Lines.Count, 1);
        Assert.Equal(nodes[1].Lines.Count, 1);
        Assert.Equal(nodes[2].Lines.Count, 1);
        Assert.Equal(nodes[0].Lines[0].ToString(), "start : . a b");
        Assert.Equal(nodes[1].Lines[0].ToString(), "start : a . b");
        Assert.Equal(nodes[2].Lines[0].ToString(), "start : a b .");
    }

    [Fact]
    public void Node4()
    {
        var nodes = RunString("start : a b a : 'A'");
        Assert.Equal(nodes.Length, 5);
        Assert.Equal(nodes[0].Lines.Count, 1);
        Assert.Equal(nodes[1].Lines.Count, 1);
        Assert.Equal(nodes[2].Lines.Count, 1);
        Assert.Equal(nodes[3].Lines.Count, 1);
        Assert.Equal(nodes[4].Lines.Count, 1);
        Assert.Equal(nodes[0].Lines[0].ToString(), "start : . a b");
        Assert.Equal(nodes[1].Lines[0].ToString(), "start : a . b");
        Assert.Equal(nodes[2].Lines[0].ToString(), "start : a b .");
        Assert.Equal(nodes[3].Lines[0].ToString(), "a : . 'A'");
        Assert.Equal(nodes[4].Lines[0].ToString(), "a : 'A' .");
    }
}
