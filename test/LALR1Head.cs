using Parser;
using Xunit;
using Yanp.Data;

namespace Yanp.Test;

public class LALR1Head
{
    private static Dictionary<string, HashSet<string>> RunString(string text, string start = "start")
    {
        var syntax = new Syntax();
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
        SyntaxParser.ParseGrammar(syntax, lex);
        SyntaxParser.GrammarsToTerminalSymbol(syntax);
        syntax.Start = start;
        var nodes = LR0.Generate(syntax);
        var lines = LALR1.GrammarLines(nodes);
        return LALR1.Head(syntax, lines);
    }

    [Fact]
    public void Head1()
    {
        var head = RunString("start :");
        Assert.True(head.ContainsKey("start"));
        Assert.Empty(head["start"]);
        Assert.Superset(head["start"], new HashSet<string> { });
    }

    [Fact]
    public void Head2()
    {
        var head = RunString("start : 'A' 'B'");
        Assert.True(head.ContainsKey("start"));
        Assert.Single(head["start"]);
        Assert.Superset(head["start"], new HashSet<string> { "'A'" });
    }

    [Fact]
    public void Head3()
    {
        var head = RunString("start : a b | c a : 'A' b : 'B' c : 'C'");
        Assert.True(head.ContainsKey("start"));
        Assert.Equal(2, head["start"].Count);
        Assert.Superset(head["start"], new HashSet<string> { "'A'", "'C'" });
        Assert.Single(head["a"]);
        Assert.Superset(head["a"], new HashSet<string> { "'A'" });
        Assert.Single(head["b"]);
        Assert.Superset(head["b"], new HashSet<string> { "'B'" });
        Assert.Single(head["c"]);
        Assert.Superset(head["c"], new HashSet<string> { "'C'" });
    }
}
