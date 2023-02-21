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
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), "");
        Assert.Equal(y.Declares.Count, 0);
        Assert.Equal(y.Grammars.Count, 0);
    }

    [Fact]
    public void Null2()
    {
        var y = RunString(@"
%%
%%");
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), "");
        Assert.Equal(y.Declares.Count, 0);
        Assert.Equal(y.Grammars.Count, 0);
    }

    [Fact]
    public void Null3()
    {
        var y = RunString(@"
%%
%%
");
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), "");
        Assert.Equal(y.Declares.Count, 0);
        Assert.Equal(y.Grammars.Count, 0);
    }

    [Fact]
    public void Comment1()
    {
        var y = RunString(@"
#%left A
%%
%%
");
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), "");
        Assert.Equal(y.Declares.Count, 0);
        Assert.Equal(y.Grammars.Count, 0);
    }

    [Fact]
    public void Comment2()
    {
        var y = RunString(@"
//%left A
%%
%%
");
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), "");
        Assert.Equal(y.Declares.Count, 0);
        Assert.Equal(y.Grammars.Count, 0);
    }

    [Fact]
    public void Comment3()
    {
        Assert.Throws<SyntaxErrorException>(() => RunString(@"
/%left A
%%
%%
"));
    }

    [Fact]
    public void Test1()
    {
        var y = RunString(@"
%start b
%%
a : 'A'
");
        Assert.Equal(y.Start, "b");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), "");
        Assert.Equal(y.Declares.Count, 2);
        Assert.Equal(y.Grammars.Count, 1);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 4, LineColumn = 1, Value = "a" }, Priority = 0, IsTerminalSymbol = false });
        Assert.Equivalent(y.Declares["'A'"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.CHAR, LineNumber = 4, LineColumn = 5, Value = "'A'" }, Priority = 0, IsTerminalSymbol = true });
        Assert.Equal(y.Grammars["a"].Count, 1);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Equal(yg.Grammars.Count, 1);
        Assert.Equivalent(yg.Grammars[0], new Token() { Type = Symbols.CHAR, LineNumber = 4, LineColumn = 5, Value = "'A'" });
    }

    [Fact]
    public void Test2()
    {
        var y = RunString(@"
%%
a : B
");
        Assert.Equal(y.Start, "a");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), "");
        Assert.Equal(y.Declares.Count, 2);
        Assert.Equal(y.Grammars.Count, 1);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 3, LineColumn = 1, Value = "a" }, Priority = 0, IsTerminalSymbol = false });
        Assert.Equivalent(y.Declares["B"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 3, LineColumn = 5, Value = "B" }, Priority = 0, IsTerminalSymbol = true });
        Assert.Equal(y.Grammars["a"].Count, 1);
        var yg = y.Grammars["a"][0];
        Assert.Null(yg.Action);
        Assert.Null(yg.Prec);
        Assert.Equal(yg.Grammars.Count, 1);
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
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), " x y z\r\n");
        Assert.Equal(y.FooterCode.ToString(), "");
        Assert.Equal(y.Declares.Count, 2);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 2, LineColumn = 7, Value = "a" }, Priority = 1, IsTerminalSymbol = true });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Right, Name = new() { Type = Symbols.VAR, LineNumber = 5, LineColumn = 8, Value = "b" }, Priority = 2, IsTerminalSymbol = true });
        Assert.Equal(y.Grammars.Count, 0);
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
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "x y z\r\n");
        Assert.Equal(y.FooterCode.ToString(), "");
        Assert.Equal(y.Declares.Count, 2);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 2, LineColumn = 7, Value = "a" }, Priority = 1, IsTerminalSymbol = true });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Right, Name = new() { Type = Symbols.VAR, LineNumber = 6, LineColumn = 8, Value = "b" }, Priority = 2, IsTerminalSymbol = true });
        Assert.Equal(y.Grammars.Count, 0);
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
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "x\r\ny\r\nz\r\n");
        Assert.Equal(y.FooterCode.ToString(), "");
        Assert.Equal(y.Declares.Count, 2);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 2, LineColumn = 7, Value = "a" }, Priority = 1, IsTerminalSymbol = true });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Right, Name = new() { Type = Symbols.VAR, LineNumber = 8, LineColumn = 8, Value = "b" }, Priority = 2, IsTerminalSymbol = true });
        Assert.Equal(y.Grammars.Count, 0);
    }

    [Fact]
    public void FooterCode1()
    {
        var y = RunString(@"
%%
%% x y z");
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), " x y z\r\n");
        Assert.Equal(y.Declares.Count, 0);
        Assert.Equal(y.Grammars.Count, 0);
    }

    [Fact]
    public void FooterCode2()
    {
        var y = RunString(@"
%%
%%
x y z");
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), "x y z");
        Assert.Equal(y.Declares.Count, 0);
        Assert.Equal(y.Grammars.Count, 0);
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
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), "x\r\ny\r\nz\r\n");
        Assert.Equal(y.Declares.Count, 0);
        Assert.Equal(y.Grammars.Count, 0);
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
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), "\r\nx\r\ny\r\nz\r\n\r\n");
        Assert.Equal(y.Declares.Count, 0);
        Assert.Equal(y.Grammars.Count, 0);
    }

    [Fact]
    public void FooterCode5()
    {
        var y = RunString(@"
%%
%%x
y z
");
        Assert.Equal(y.Start, "");
        Assert.Equal(y.HeaderCode.ToString(), "");
        Assert.Equal(y.FooterCode.ToString(), "x\r\ny z\r\n");
        Assert.Equal(y.Declares.Count, 0);
        Assert.Equal(y.Grammars.Count, 0);
    }
}
