using Parser;
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
        Assert.Equal("", y.Start);
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
        Assert.Equal("", y.Start);
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
        Assert.Equal("", y.Start);
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("\r\n", y.FooterCode);
        Assert.Empty(y.Declares);
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void Comment1()
    {
        var y = RunString(@"
#%left A
%%
%%
");
        Assert.Equal("", y.Start);
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("\r\n", y.FooterCode);
        Assert.Empty(y.Declares);
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void Test1()
    {
        var y = RunString(@"
%start b
%%
a : 'A'
");
        Assert.Equal("b", y.Start);
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("", y.FooterCode);
        Assert.Equal(2, y.Declares.Count);
        _ = Assert.Single(y.Grammars);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 4, LineColumn = 1, Value = "a" }, Priority = 0, IsTerminalSymbol = false });
        Assert.Equivalent(y.Declares["'A"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.CHAR, LineNumber = 4, LineColumn = 5, Value = "'A" }, Priority = 0, IsTerminalSymbol = true });
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        _ = Assert.Single(yg.Grammars);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.CHAR, LineNumber = 4, LineColumn = 5, Value = "'A" });
    }

    [Fact]
    public void Test2()
    {
        var y = RunString(@"
%%
a : B
");
        Assert.Equal("a", y.Start);
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("", y.FooterCode);
        Assert.Equal(2, y.Declares.Count);
        _ = Assert.Single(y.Grammars);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 3, LineColumn = 1, Value = "a" }, Priority = 0, IsTerminalSymbol = false });
        Assert.Equivalent(y.Declares["B"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 3, LineColumn = 5, Value = "B" }, Priority = 0, IsTerminalSymbol = true });
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        _ = Assert.Single(yg.Grammars);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.VAR, LineNumber = 3, LineColumn = 5, Value = "B" });
    }
}
