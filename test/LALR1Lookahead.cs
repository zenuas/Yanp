using Parser;
using Xunit;
using Yanp.Data;

namespace Yanp.Test;

public class LALR1Lookahead
{
    private static Node[] RunString(string text, string start = "start")
    {
        var syntax = new Syntax();
        var lex = new Lexer { BaseReader = new() { BaseReader = new StringReader(text) } };
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
        Assert.Equal(nodes.Length, 8);
        Assert.Equal(nodes[0].ToString(), "$ACCEPT : . start $END; start : . left '=' right; left : . right; start : . right; right : . ID");
        Assert.Equal(nodes[1].ToString(), "$ACCEPT : start . $END");
        Assert.Equal(nodes[2].ToString(), "$ACCEPT : start $END .");
        Assert.Equal(nodes[3].ToString(), "start : left . '=' right");
        Assert.Equal(nodes[4].ToString(), "start : left '=' . right; right : . ID");
        Assert.Equal(nodes[5].ToString(), "start : left '=' right . [$END]");
        Assert.Equal(nodes[6].ToString(), "right : ID . ['=', $END]");
        Assert.Equal(nodes[7].ToString(), "left : right . ['=']; start : right . [$END]");
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
        Assert.Equal(nodes.Length, 11);
        Assert.Equal(nodes[0].ToString(), "$ACCEPT : . start $END; start : . a bvoid c; a : . 'A'; start : . bvoid; bvoid : . void; bvoid : . 'B'; void : . [$END]");
        Assert.Equal(nodes[1].ToString(), "$ACCEPT : start . $END");
        Assert.Equal(nodes[2].ToString(), "$ACCEPT : start $END .");
        Assert.Equal(nodes[3].ToString(), "start : a . bvoid c; bvoid : . void; bvoid : . 'B'; void : . ['C']");
        Assert.Equal(nodes[4].ToString(), "start : a bvoid . c; c : . 'C'");
        Assert.Equal(nodes[5].ToString(), "start : a bvoid c . [$END]");
        Assert.Equal(nodes[6].ToString(), "c : 'C' . [$END]");
        Assert.Equal(nodes[7].ToString(), "bvoid : void . ['C', $END]");
        Assert.Equal(nodes[8].ToString(), "bvoid : 'B' . ['C', $END]");
        Assert.Equal(nodes[9].ToString(), "a : 'A' . ['B', 'C', $END]");
        Assert.Equal(nodes[10].ToString(), "start : bvoid . [$END]");
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
        Assert.Equal(nodes.Length, 10);
        Assert.Equal(nodes[0].ToString(), "$ACCEPT : . start $END; start : . stmt; stmt : . void; stmt : . stmt line; void : . ['A', 'B', $END]");
        Assert.Equal(nodes[1].ToString(), "$ACCEPT : start . $END");
        Assert.Equal(nodes[2].ToString(), "$ACCEPT : start $END .");
        Assert.Equal(nodes[3].ToString(), "stmt : void . ['A', 'B', $END]");
        Assert.Equal(nodes[4].ToString(), "start : stmt . [$END]; stmt : stmt . line; line : . a; line : . b; a : . 'A'; b : . 'B'");
        Assert.Equal(nodes[5].ToString(), "stmt : stmt line . ['A', 'B', $END]");
        Assert.Equal(nodes[6].ToString(), "line : a . ['A', 'B', $END]");
        Assert.Equal(nodes[7].ToString(), "line : b . ['A', 'B', $END]");
        Assert.Equal(nodes[8].ToString(), "a : 'A' . ['A', 'B', $END]");
        Assert.Equal(nodes[9].ToString(), "b : 'B' . ['A', 'B', $END]");
    }

    [Fact]
    public void Lookahead4()
    {

        var nodes = RunString(@"
start : avoid avoid bvoid
void  :
avoid : void
      | 'A'
bvoid : void
      | 'B'
");
        Assert.Equal(nodes.Length, 10);
        Assert.Equal(nodes[0].ToString(), "$ACCEPT : . start $END; start : . avoid avoid bvoid; avoid : . void; avoid : . 'A'; void : . ['A', 'B', $END]");
        Assert.Equal(nodes[1].ToString(), "$ACCEPT : start . $END");
        Assert.Equal(nodes[2].ToString(), "$ACCEPT : start $END .");
        Assert.Equal(nodes[3].ToString(), "start : avoid . avoid bvoid; avoid : . void; avoid : . 'A'; void : . ['B', $END]");
        Assert.Equal(nodes[4].ToString(), "start : avoid avoid . bvoid; bvoid : . void; bvoid : . 'B'; void : . [$END]");
        Assert.Equal(nodes[5].ToString(), "start : avoid avoid bvoid . [$END]");
        Assert.Equal(nodes[6].ToString(), "bvoid : void . [$END]");
        Assert.Equal(nodes[7].ToString(), "bvoid : 'B' . [$END]");
        Assert.Equal(nodes[8].ToString(), "avoid : void . ['A', 'B', $END]");
        Assert.Equal(nodes[9].ToString(), "avoid : 'A' . ['A', 'B', $END]");

    }

    [Fact]
    public void Lookahead5()
    {

        var nodes = RunString(@"
start : 'A' void 'B'
      | void 'C'
void  :
");
        Assert.Equal(nodes.Length, 8);
        Assert.Equal(nodes[0].ToString(), "$ACCEPT : . start $END; start : . 'A' void 'B'; start : . void 'C'; void : . ['C']");
        Assert.Equal(nodes[1].ToString(), "$ACCEPT : start . $END");
        Assert.Equal(nodes[2].ToString(), "$ACCEPT : start $END .");
        Assert.Equal(nodes[3].ToString(), "start : 'A' . void 'B'; void : . ['B']");
        Assert.Equal(nodes[4].ToString(), "start : 'A' void . 'B'");
        Assert.Equal(nodes[5].ToString(), "start : 'A' void 'B' . [$END]");
        Assert.Equal(nodes[6].ToString(), "start : void . 'C'");
        Assert.Equal(nodes[7].ToString(), "start : void 'C' . [$END]");
    }

    [Fact]
    public void Lookahead6()
    {

        var nodes = RunString(@"
start :
");
        Assert.Equal(nodes.Length, 3);
        Assert.Equal(nodes[0].ToString(), "$ACCEPT : . start $END; start : . [$END]");
        Assert.Equal(nodes[1].ToString(), "$ACCEPT : start . $END");
        Assert.Equal(nodes[2].ToString(), "$ACCEPT : start $END .");
    }
}
