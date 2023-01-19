using Parser;
using Xunit;
using Yanp;

public class ParseDeclaration
{
    private static Syntax RunString(string text)
    {
        var syntax = new Syntax();
        var lex = new Lexer(new() { BaseReader = new StringReader(text) });
        ParserGenerator.ParseDeclaration(syntax, lex);
        return syntax;
    }

    [Fact]
    public void Null()
    {
        var y = RunString("");
        Assert.Empty(y.Declares);
    }

    [Fact]
    public void Left()
    {
        var y = RunString("%left a");
        _ = Assert.Single(y.Declares);
        Assert.Equivalent(y.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = "a", Priority = 1 });

        var y2 = RunString("%left a b");
        Assert.Equal(2, y2.Declares.Count);
        Assert.Equivalent(y2.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = "a", Priority = 1 });
        Assert.Equivalent(y2.Declares["b"], new Declarate() { Assoc = AssocTypes.Left, Name = "b", Priority = 1 });

        var y3 = RunString("%left a b %left c");
        Assert.Equal(3, y3.Declares.Count);
        Assert.Equivalent(y3.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = "a", Priority = 1 });
        Assert.Equivalent(y3.Declares["b"], new Declarate() { Assoc = AssocTypes.Left, Name = "b", Priority = 1 });
        Assert.Equivalent(y3.Declares["c"], new Declarate() { Assoc = AssocTypes.Left, Name = "c", Priority = 2 });
    }

    [Fact]
    public void Type()
    {
        _ = Assert.Throws<SyntaxErrorException>(() => RunString("%type a"));

        var y2 = RunString("%type<type> a b");
        Assert.Equal(2, y2.Declares.Count);
        Assert.Equivalent(y2.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = "a", Priority = 1, Type = "type" });
        Assert.Equivalent(y2.Declares["b"], new Declarate() { Assoc = AssocTypes.Type, Name = "b", Priority = 1, Type = "type" });

        var y3 = RunString("%type<type> a b %type<type2> c");
        Assert.Equal(3, y3.Declares.Count);
        Assert.Equivalent(y3.Declares["a"], new Declarate() { Assoc = AssocTypes.Type, Name = "a", Priority = 1, Type = "type" });
        Assert.Equivalent(y3.Declares["b"], new Declarate() { Assoc = AssocTypes.Type, Name = "b", Priority = 1, Type = "type" });
        Assert.Equivalent(y3.Declares["c"], new Declarate() { Assoc = AssocTypes.Type, Name = "c", Priority = 2, Type = "type2" });
    }

    [Fact]
    public void Char()
    {
        var y = RunString("%left 'a'");
        _ = Assert.Single(y.Declares);
        Assert.Equivalent(y.Declares["'a"], new Declarate() { Assoc = AssocTypes.Left, Name = "'a", Priority = 1 });

        var y2 = RunString("%left 'a' a");
        Assert.Equal(2, y2.Declares.Count);
        Assert.Equivalent(y2.Declares["'a"], new Declarate() { Assoc = AssocTypes.Left, Name = "'a", Priority = 1 });
        Assert.Equivalent(y2.Declares["a"], new Declarate() { Assoc = AssocTypes.Left, Name = "a", Priority = 1 });
    }
}
