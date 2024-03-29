﻿using Xunit;
using Yanp.Data;
using Yanp.Parser;

namespace Yanp.Test;

public class LALR1Follow
{
    private static Dictionary<string, HashSet<string>> RunString(string text, string start = "start")
    {
        var syntax = new Syntax();
        var lex = new Lexer { BaseReader = new() { BaseReader = new StringReader(text) } };
        SyntaxParser.ParseGrammar(syntax, lex);
        SyntaxParser.GrammarsToTerminalSymbol(syntax);
        syntax.Start = start;
        var nodes = LR0.Generate(syntax);
        var lines = LALR1.GrammarLines(nodes);
        return LALR1.Follow(nodes, lines, LALR1.Nullable(lines));
    }

    [Fact]
    public void Follow1()
    {
        var follow = RunString("start : void a b a : 'A' b : 'B' void :");
        Assert.Equal(follow.Count, 4);
        Assert.Superset(follow["start"], new HashSet<string> { "$END" });
        Assert.Superset(follow["a"], new HashSet<string> { "'B'" });
        Assert.Superset(follow["b"], new HashSet<string> { "$END" });
        Assert.Superset(follow["void"], new HashSet<string> { "'A'" });
    }

    [Fact]
    public void Follow2()
    {
        var follow = RunString("start : a void void2 a : 'A' void : void2 :");
        Assert.Equal(follow.Count, 4);
        Assert.Superset(follow["start"], new HashSet<string> { "$END" });
        Assert.Superset(follow["a"], new HashSet<string> { "$END" });
        Assert.Superset(follow["void"], new HashSet<string> { "$END" });
        Assert.Superset(follow["void2"], new HashSet<string> { "$END" });
    }

    [Fact]
    public void Follow3()
    {
        var follow = RunString("start : 'A' void 'B' | 'C' void 'D' void :");
        Assert.Equal(follow.Count, 2);
        Assert.Superset(follow["start"], new HashSet<string> { "$END" });
        Assert.Superset(follow["void"], new HashSet<string> { "'B'", "'D'" });
    }

    [Fact]
    public void Follow4()
    {
        var follow = RunString("start : avoid avoid bvoid void : avoid : void | 'A' bvoid : void | 'B'");
        Assert.Equal(follow.Count, 4);
        Assert.Superset(follow["start"], new HashSet<string> { "$END" });
        Assert.Superset(follow["void"], new HashSet<string> { "'A'", "'B'", "$END" });
        Assert.Superset(follow["avoid"], new HashSet<string> { "'A'", "'B'", "$END" });
        Assert.Superset(follow["bvoid"], new HashSet<string> { "$END" });
    }
}
