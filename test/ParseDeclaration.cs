using Parser;
using Xunit;
using Yanp.Data;

namespace Yanp.Test;

public class ParseDeclaration
{
    private static Syntax RunString(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
        Yanp.SyntaxParser.ParseDeclaration(syntax, lex);
        return syntax;
    }

    [Fact]
    public void Null()
    {
        var y = RunString("");
        Assert.Empty(y.Declares);
    }

    [Fact]
    public void Left1()
    {
        var y = RunString("%left a");
        _ = Assert.Single(y.Declares);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "a" }, Priority = 1 });
    }

    [Fact]
    public void Left2()
    {
        var y = RunString("%left a b");
        Assert.Equal(2, y.Declares.Count);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "a" }, Priority = 1 });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 9, Value = "b" }, Priority = 1 });
    }

    [Fact]
    public void Left3()
    {
        var y = RunString("%left a b %left c");
        Assert.Equal(3, y.Declares.Count);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "a" }, Priority = 1 });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 9, Value = "b" }, Priority = 1 });
        Assert.Equivalent(y.Declares["c"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 17, Value = "c" }, Priority = 2 });
    }

    [Fact]
    public void Type1()
    {
        _ = Assert.Throws<SyntaxErrorException>(() => RunString("%type a"));
    }

    [Fact]
    public void Type2()
    {
        var y = RunString("%type<type> a b");
        Assert.Equal(2, y.Declares.Count);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 13, Value = "a" }, Priority = 1, Type = "type" });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 15, Value = "b" }, Priority = 1, Type = "type" });
    }

    [Fact]
    public void Type3()
    {
        var y = RunString("%type<type> a b %type<type2> c");
        Assert.Equal(3, y.Declares.Count);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 13, Value = "a" }, Priority = 1, Type = "type" });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 15, Value = "b" }, Priority = 1, Type = "type" });
        Assert.Equivalent(y.Declares["c"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 30, Value = "c" }, Priority = 2, Type = "type2" });
    }

    [Fact]
    public void Char1()
    {
        var y = RunString("%left 'a'");
        _ = Assert.Single(y.Declares);
        Assert.Equivalent(y.Declares["'a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 7, Value = "'a" }, Priority = 1 });
    }

    [Fact]
    public void Char2()
    {
        var y = RunString("%left 'a' a");
        Assert.Equal(2, y.Declares.Count);
        Assert.Equivalent(y.Declares["'a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 7, Value = "'a" }, Priority = 1 });
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 11, Value = "a" }, Priority = 1 });
    }

    [Fact]
    public void Inline()
    {
        var y = RunString(@"
%token a
%{
hello
world
%}
%token b
");
        Assert.Equal(2, y.Declares.Count);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 2, LineColumn = 8, Value = "a" }, Priority = 1 });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 7, LineColumn = 8, Value = "b" }, Priority = 2 });
        Assert.Equal("hello\r\nworld\r\n", y.HeaderCode.ToString());
    }

    [Fact]
    public void Default1()
    {
        _ = Assert.Throws<SyntaxErrorException>(() => RunString("%default 'a'"));
    }

    [Fact]
    public void Default2()
    {
        var y = RunString("%default a");
        Assert.Equal("a", y.Default);
    }

    [Fact]
    public void Default3()
    {
        var y = RunString("%default a %default b");
        Assert.Equal("b", y.Default);
    }

    [Fact]
    public void Define1()
    {
        _ = Assert.Throws<SyntaxErrorException>(() => RunString("%define 'a'"));
    }

    [Fact]
    public void Define2()
    {
        var y = RunString("%define a");
        Assert.Equal("", y.Defines["a"]);
    }

    [Fact]
    public void Define3()
    {
        var y = RunString("%define a b");
        _ = Assert.Single(y.Defines);
        Assert.Equal("b", y.Defines["a"]);
    }

    [Fact]
    public void Define4()
    {
        _ = Assert.Throws<ArgumentException>(() => RunString(@"
%define a b
%define a c
"));
    }

    [Fact]
    public void Define5()
    {
        var y = RunString(@"
%define a b
%define c d
");
        Assert.Equal(2, y.Defines.Count);
        Assert.Equal("b", y.Defines["a"]);
        Assert.Equal("d", y.Defines["c"]);
    }

    [Fact]
    public void Start1()
    {
        _ = Assert.Throws<SyntaxErrorException>(() => RunString("%start"));
    }

    [Fact]
    public void Start2()
    {
        _ = Assert.Throws<SyntaxErrorException>(() => RunString("%start 'a'"));
    }

    [Fact]
    public void Start3()
    {
        var y = RunString("%start a");
        Assert.Equal("a", y.Start);
    }

    [Fact]
    public void Start4()
    {
        var y = RunString("%start a %start b");
        Assert.Equal("b", y.Start);
    }
}
