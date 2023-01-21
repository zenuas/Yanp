using Xunit;
using Yanp;

public class SyntaxParser
{
    private static Syntax RunString(string text) => Yanp.SyntaxParser.Parse(new StringReader(text));

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
