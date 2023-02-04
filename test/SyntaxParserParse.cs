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
        Assert.Equal("", y.FooterCode.ToString());
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
        Assert.Equal("", y.FooterCode.ToString());
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
        Assert.Equal("", y.FooterCode.ToString());
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
        Assert.Equal("", y.FooterCode.ToString());
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
        Assert.Equal("", y.FooterCode.ToString());
        Assert.Equal(2, y.Declares.Count);
        _ = Assert.Single(y.Grammars);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 4, LineColumn = 1, Value = "a" }, Priority = 0, IsTerminalSymbol = false });
        Assert.Equivalent(y.Declares["'A'"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.CHAR, LineNumber = 4, LineColumn = 5, Value = "'A'" }, Priority = 0, IsTerminalSymbol = true });
        _ = Assert.Single(y.Grammars["a"]);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        _ = Assert.Single(yg.Grammars);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.CHAR, LineNumber = 4, LineColumn = 5, Value = "'A'" });
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
        Assert.Equal("", y.FooterCode.ToString());
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

    [Fact]
    public void HeaderCode1()
    {
        var y = RunString(@"
%left a
%{ x y z
%}
%right b
%%
");
        Assert.Equal("", y.Start);
        Assert.Equal(" x y z\r\n", y.HeaderCode.ToString());
        Assert.Equal("", y.FooterCode.ToString());
        Assert.Equal(2, y.Declares.Count);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 2, LineColumn = 7, Value = "a" }, Priority = 1, IsTerminalSymbol = true });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Right, Name = new() { Type = Symbols.VAR, LineNumber = 5, LineColumn = 8, Value = "b" }, Priority = 2, IsTerminalSymbol = true });
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void HeaderCode2()
    {
        var y = RunString(@"
%left a
%{
x y z
%}
%right b
%%
");
        Assert.Equal("", y.Start);
        Assert.Equal("x y z\r\n", y.HeaderCode.ToString());
        Assert.Equal("", y.FooterCode.ToString());
        Assert.Equal(2, y.Declares.Count);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 2, LineColumn = 7, Value = "a" }, Priority = 1, IsTerminalSymbol = true });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Right, Name = new() { Type = Symbols.VAR, LineNumber = 6, LineColumn = 8, Value = "b" }, Priority = 2, IsTerminalSymbol = true });
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void HeaderCode3()
    {
        var y = RunString(@"
%left a
%{
x
y
z
%}
%right b
%%
");
        Assert.Equal("", y.Start);
        Assert.Equal("x\r\ny\r\nz\r\n", y.HeaderCode.ToString());
        Assert.Equal("", y.FooterCode.ToString());
        Assert.Equal(2, y.Declares.Count);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 2, LineColumn = 7, Value = "a" }, Priority = 1, IsTerminalSymbol = true });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Right, Name = new() { Type = Symbols.VAR, LineNumber = 8, LineColumn = 8, Value = "b" }, Priority = 2, IsTerminalSymbol = true });
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void FooterCode1()
    {
        var y = RunString(@"
%%
%% x y z");
        Assert.Equal("", y.Start);
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal(" x y z\r\n", y.FooterCode.ToString());
        Assert.Empty(y.Declares);
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void FooterCode2()
    {
        var y = RunString(@"
%%
%%
x y z");
        Assert.Equal("", y.Start);
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("x y z", y.FooterCode.ToString());
        Assert.Empty(y.Declares);
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void FooterCode3()
    {
        var y = RunString(@"
%%
%%
x
y
z
");
        Assert.Equal("", y.Start);
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("x\r\ny\r\nz\r\n", y.FooterCode.ToString());
        Assert.Empty(y.Declares);
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void FooterCode4()
    {
        var y = RunString(@"
%%
%%

x
y
z

");
        Assert.Equal("", y.Start);
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("\r\nx\r\ny\r\nz\r\n\r\n", y.FooterCode.ToString());
        Assert.Empty(y.Declares);
        Assert.Empty(y.Grammars);
    }

    [Fact]
    public void FooterCode5()
    {
        var y = RunString(@"
%%
%%x
y z
");
        Assert.Equal("", y.Start);
        Assert.Equal("", y.HeaderCode.ToString());
        Assert.Equal("x\r\ny z\r\n", y.FooterCode.ToString());
        Assert.Empty(y.Declares);
        Assert.Empty(y.Grammars);
    }
}
