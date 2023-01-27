using Xunit;
using Yanp.Data;

namespace Yanp.Test;

public class SyntaxParserParse
{
    private static Syntax RunString(string text) => SyntaxParser.Parse(new StringReader(text));

    [Fact]
    public void Null1()
    {
        var y = RunString(@"
%%
");
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("", y.FooterCode);
        Assert.Empty(y.Declares);
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void Null2()
    {
        var y = RunString(@"
%%
%%");
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("", y.FooterCode);
        Assert.Empty(y.Declares);
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void Null3()
    {
        var y = RunString(@"
%%
%%
");
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("\r\n", y.FooterCode);
        Assert.Empty(y.Declares);
        Assert.Empty(y.Grammars);
    }
}
