using Xunit;
using Yanp.Data;
using Yanp.Parser;

namespace Yanp.Test;

public class SyntaxParserParseDeclaration
{
    private static Syntax RunString(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer { BaseReader = new() { BaseReader = new StringReader(text) } };
        SyntaxParser.ParseDeclaration(syntax, lex);
        return syntax;
    }

    [Fact]
    public void Null()
    {
        var y = RunString("");
        Assert.Equal(y.Declares.Count, 0);
    }

    [Fact]
    public void Left1()
    {
        var y = RunString("%left a");
        Assert.Equal(y.Declares.Count, 1);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "a" }, Priority = 1 });
    }

    [Fact]
    public void Left2()
    {
        var y = RunString("%left a b");
        Assert.Equal(y.Declares.Count, 2);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 7, Value = "a" }, Priority = 1 });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 9, Value = "b" }, Priority = 1 });
    }

    [Fact]
    public void Left3()
    {
        var y = RunString("%left a b %left c");
        Assert.Equal(y.Declares.Count, 3);
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
        Assert.Equal(y.Declares.Count, 2);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 13, Value = "a" }, Priority = 1, Type = "type" });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 15, Value = "b" }, Priority = 1, Type = "type" });
    }

    [Fact]
    public void Type3()
    {
        var y = RunString("%type<type> a b %type<type2> c");
        Assert.Equal(y.Declares.Count, 3);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 13, Value = "a" }, Priority = 1, Type = "type" });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 15, Value = "b" }, Priority = 1, Type = "type" });
        Assert.Equivalent(y.Declares["c"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 1, LineColumn = 30, Value = "c" }, Priority = 2, Type = "type2" });
    }

    [Fact]
    public void Char1()
    {
        var y = RunString("%left 'a'");
        Assert.Equal(y.Declares.Count, 1);
        Assert.Equivalent(y.Declares["'a'"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 7, Value = "'a'" }, Priority = 1 });
    }

    [Fact]
    public void Char2()
    {
        var y = RunString("%left 'a' a");
        Assert.Equal(y.Declares.Count, 2);
        Assert.Equivalent(y.Declares["'a'"], new Declarate() { Assoc = AssocTypes.Left, Name = new() { Type = Symbols.CHAR, LineNumber = 1, LineColumn = 7, Value = "'a'" }, Priority = 1 });
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
        Assert.Equal(y.Declares.Count, 2);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 2, LineColumn = 8, Value = "a" }, Priority = 1 });
        Assert.Equivalent(y.Declares["b"], new Declarate() { Assoc = AssocTypes.Type, Name = new() { Type = Symbols.VAR, LineNumber = 7, LineColumn = 8, Value = "b" }, Priority = 2 });
        Assert.Equal(y.HeaderCode.ToString(), "hello\r\nworld\r\n");
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
        Assert.Equal(y.Default, "a");
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
        Assert.Equal(y.Defines["a"], "");
    }

    [Fact]
    public void Define3()
    {
        var y = RunString("%define a b");
        Assert.Equal(y.Defines.Count, 1);
        Assert.Equal(y.Defines["a"], "b");
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
        Assert.Equal(y.Defines.Count, 2);
        Assert.Equal(y.Defines["a"], "b");
        Assert.Equal(y.Defines["c"], "d");
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
        Assert.Equal(y.Start, "a");
    }

    [Fact]
    public void Start4()
    {
        var y = RunString("%start a %start b");
        Assert.Equal(y.Start, "b");
    }
}
