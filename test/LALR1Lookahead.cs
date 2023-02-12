using Parser;
using Xunit;
using Yanp.Data;

namespace Yanp.Test;

public class LALR1Lookahead
{
    private static Node[] RunString(string text, string start = "start")
    {
        var syntax = new Syntax();
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
        SyntaxParser.ParseGrammar(syntax, lex);
        SyntaxParser.GrammarsToTerminalSymbol(syntax);
        syntax.Start = start;
        var nodes = LR0.Generate(syntax);
        LALR1.Generate(syntax, nodes);
        return nodes;
    }

    [Fact]
    public void Lookahead1()
    {
        var nodes = RunString(@"
start : left '=' right 
      | right
left  : right
right : ID
");
        Assert.Equal(8, nodes.Length);
        Assert.Equal("$ACCEPT : . start $END; start : . left '=' right; left : . right; start : . right; right : . ID", nodes[0].ToString());
        Assert.Equal("$ACCEPT : start . $END", nodes[1].ToString());
        Assert.Equal("$ACCEPT : start $END .", nodes[2].ToString());
        Assert.Equal("start : left . '=' right", nodes[3].ToString());
        Assert.Equal("start : left '=' . right; right : . ID", nodes[4].ToString());
        Assert.Equal("start : left '=' right . [$END]", nodes[5].ToString());
        Assert.Equal("right : ID . ['=', $END]", nodes[6].ToString());
        Assert.Equal("left : right . ['=']; start : right . [$END]", nodes[7].ToString());
    }

    [Fact]
    public void Lookahead2()
    {
        var nodes = RunString(@"
start : a bvoid c
      | bvoid
a     : 'A'
bvoid : void
      | 'B'
c     : 'C'
void  :
");
        Assert.Equal(11, nodes.Length);
        Assert.Equal("$ACCEPT : . start $END; start : . a bvoid c; a : . 'A'; start : . bvoid; bvoid : . void; bvoid : . 'B'; void : . [$END]", nodes[0].ToString());
        Assert.Equal("$ACCEPT : start . $END", nodes[1].ToString());
        Assert.Equal("$ACCEPT : start $END .", nodes[2].ToString());
        Assert.Equal("start : a . bvoid c; bvoid : . void; bvoid : . 'B'; void : . ['C']", nodes[3].ToString());
        Assert.Equal("start : a bvoid . c; c : . 'C'", nodes[4].ToString());
        Assert.Equal("start : a bvoid c . [$END]", nodes[5].ToString());
        Assert.Equal("c : 'C' . [$END]", nodes[6].ToString());
        Assert.Equal("bvoid : void . ['C', $END]", nodes[7].ToString());
        Assert.Equal("bvoid : 'B' . ['C', $END]", nodes[8].ToString());
        Assert.Equal("a : 'A' . ['B', 'C', $END]", nodes[9].ToString());
        Assert.Equal("start : bvoid . [$END]", nodes[10].ToString());
    }

    [Fact]
    public void Lookahead3()
    {
        var nodes = RunString(@"
start : stmt
stmt  : void
      | stmt line
line  : a
      | b
a     : 'A'
b     : 'B'
void  :
");
        Assert.Equal(10, nodes.Length);
        Assert.Equal("$ACCEPT : . start $END; start : . stmt; stmt : . void; stmt : . stmt line; void : . ['A', 'B', $END]", nodes[0].ToString());
        Assert.Equal("$ACCEPT : start . $END", nodes[1].ToString());
        Assert.Equal("$ACCEPT : start $END .", nodes[2].ToString());
        Assert.Equal("stmt : void . ['A', 'B', $END]", nodes[3].ToString());
        Assert.Equal("start : stmt . [$END]; stmt : stmt . line; line : . a; line : . b; a : . 'A'; b : . 'B'", nodes[4].ToString());
        Assert.Equal("stmt : stmt line . ['A', 'B', $END]", nodes[5].ToString());
        Assert.Equal("line : a . ['A', 'B', $END]", nodes[6].ToString());
        Assert.Equal("line : b . ['A', 'B', $END]", nodes[7].ToString());
        Assert.Equal("a : 'A' . ['A', 'B', $END]", nodes[8].ToString());
        Assert.Equal("b : 'B' . ['A', 'B', $END]", nodes[9].ToString());
    }
}
