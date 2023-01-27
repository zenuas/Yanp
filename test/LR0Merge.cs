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
        syntax.Start = start;
        return LR0.Generate(syntax);
    }

    [Fact]
    public void Merge1()
    {
        var nodes = RunString("start : a b | a c");
        Assert.Equal(6, nodes.Length);
        Assert.Equal("$ACCEPT : . start $END; start : . a b; start : . a c", nodes[0].ToString());
        Assert.Equal("$ACCEPT : start . $END", nodes[1].ToString());
        Assert.Equal("$ACCEPT : start $END .", nodes[2].ToString());
        Assert.Equal("start : a . b; start : a . c", nodes[3].ToString());
        Assert.Equal("start : a b .", nodes[4].ToString());
        Assert.Equal("start : a c .", nodes[5].ToString());
    }
}
