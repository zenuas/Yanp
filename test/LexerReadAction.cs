using Parser;
using Xunit;

namespace Yanp.Test;

public class LexerReadAction
{
    private static string RunString(string text) => RunString2(text).Action;

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
        Assert.Equal(RunString("{}"), "");
        Assert.Equal(RunString("{ }"), " ");
        Assert.Equal(RunString("{ { a } }"), " { a } ");
        Assert.Equal(RunString("{ 'abc' }"), " 'abc' ");
        Assert.Equal(RunString("{ '\"' }"), " '\"' ");
        Assert.Equal(RunString("{ '\\'' }"), " '\\'' ");
        Assert.Equal(RunString("{ '{' }"), " '{' ");

        var a = RunString2("{ if(true == false } a");
        Assert.Equal(a.Action, " if(true == false ");
        Assert.Equal(a.Reader.LineNumber, 1);
        Assert.Equal(a.Reader.LineColumn, 21);
    }
}
