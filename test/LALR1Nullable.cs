﻿using Xunit;
using Yanp.Data;
using Yanp.Parser;

namespace Yanp.Test;

public class LALR1Nullable
{
    private static HashSet<string> RunString(string text, string start = "start")
    {
        var syntax = new Syntax();
        var lex = new Lexer { BaseReader = new() { BaseReader = new StringReader(text) } };
        SyntaxParser.ParseGrammar(syntax, lex);
        SyntaxParser.GrammarsToTerminalSymbol(syntax);
        syntax.Start = start;
        var nodes = LR0.Generate(syntax);
        var lines = LALR1.GrammarLines(nodes);
        return LALR1.Nullable(lines);
    }

    [Fact]
    public void Nullable1()
    {
        var hash = RunString("start :");
        Assert.Equal(hash, ["start"]);
    }

    [Fact]
    public void Nullable2()
    {
        var hash = RunString("start : a 'B' a :");
        Assert.Equal(hash, ["a"]);
    }

    [Fact]
    public void Nullable3()
    {
        var hash = RunString("start : a b 'C' a : void b : void void :");
        Assert.Equal(hash, ["a", "b", "void"]);
    }
}
