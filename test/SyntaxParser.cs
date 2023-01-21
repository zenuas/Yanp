using Xunit;
using Yanp;

public class SyntaxParser
{
    private static Syntax RunString(string text) => Yanp.SyntaxParser.Parse(new StringReader(text));

    [Fact]
    public void Null()
    {
        var y = RunString(@"
%%
");
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("", y.FooterCode);
        Assert.Empty(y.Declares);
        Assert.Empty(y.Grammars);

        var y2 = RunString(@"
%%
%%");
        Assert.Equal("", y2.HeaderCode.ToString());
        Assert.Equal("", y2.FooterCode);
        Assert.Empty(y2.Declares);
        Assert.Empty(y2.Grammars);

        var y3 = RunString(@"
%%
%%
");
        Assert.Equal("", y3.HeaderCode.ToString());
        Assert.Equal("\r\n", y3.FooterCode);
        Assert.Empty(y3.Declares);
        Assert.Empty(y3.Grammars);
    }
}
