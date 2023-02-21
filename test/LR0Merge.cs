using Parser;
using Xunit;
using Yanp.Data;

namespace Yanp.Test;

public class LR0Merge
{
    private static Node[] RunString(string text, string start = "start")
    {
        var syntax = new Syntax();
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
        SyntaxParser.ParseGrammar(syntax, lex);
        SyntaxParser.GrammarsToTerminalSymbol(syntax);
        syntax.Start = start;
        return LR0.Generate(syntax);
    }

    [Fact]
    public void Merge1()
    {
        var nodes = RunString("start : a b | a c");
        Assert.Equal(nodes.Length, 6);
        Assert.Equal(nodes[0].ToString(), "$ACCEPT : . start $END; start : . a b; start : . a c");
        Assert.Equal(nodes[1].ToString(), "$ACCEPT : start . $END");
        Assert.Equal(nodes[2].ToString(), "$ACCEPT : start $END .");
        Assert.Equal(nodes[3].ToString(), "start : a . b; start : a . c");
        Assert.Equal(nodes[4].ToString(), "start : a b .");
        Assert.Equal(nodes[5].ToString(), "start : a c .");
    }
}
