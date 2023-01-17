﻿using Parser;
using Xunit;

public class ReadAction
{
    private static string RunString(string text)
    {
        return RunString2(text).Action;
    }
    private static (string Action, SourceCodeReader Reader) RunString2(string text)
    {
        var reader = new SourceCodeReader() { BaseReader = new StringReader(text) };
        return (Lexer.ReadAction(reader), reader);
    }

    [Fact]
    public void Null()
    {
        _ = Assert.Throws<SyntaxErrorException>(() => RunString(""));
    }

    [Fact]
    public void Error()
    {
        _ = Assert.Throws<SyntaxErrorException>(() => RunString("{a"));
        _ = Assert.Throws<SyntaxErrorException>(() => RunString("{ { }"));
        _ = Assert.Throws<SyntaxErrorException>(() => RunString("{ '}'"));
    }

    [Fact]
    public void Action()
    {
        Assert.Equal("", RunString("{}"));
        Assert.Equal(" ", RunString("{ }"));
        Assert.Equal(" { a } ", RunString("{ { a } }"));
        Assert.Equal(" 'abc' ", RunString("{ 'abc' }"));
        Assert.Equal(" '\"' ", RunString("{ '\"' }"));
        Assert.Equal(" '\\'' ", RunString("{ '\\'' }"));
        Assert.Equal(" '{' ", RunString("{ '{' }"));

        var a = RunString2("{ if(true == false } a");
        Assert.Equal(" if(true == false ", a.Action);
        Assert.Equal(1, a.Reader.LineNumber);
        Assert.Equal(21, a.Reader.LineColumn);
    }
}
