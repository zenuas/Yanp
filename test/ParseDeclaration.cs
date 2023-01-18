using Parser;
using Xunit;
using Yanp;

public class ParseDeclaration
{
    public class EqDeclarate : IEqualityComparer<Declarate>
    {
        public bool Equals(Declarate? x, Declarate? y)
        {
            return x?.Name == y?.Name &&
                x?.Assoc == y?.Assoc &&
                x?.Priority == y?.Priority &&
                x?.Type == y?.Type &&
                x?.IsTerminalSymbol == y?.IsTerminalSymbol &&
                x?.IsAction == y?.IsAction;
        }

        public int GetHashCode(Declarate obj) => obj.GetHashCode();
    }

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
        Assert.Single(y.Declares);
        Assert.Equal(y.Declares["a"], new() { Assoc = AssocTypes.Left, Name = "a", Priority = 1 }, new EqDeclarate());

        var y2 = RunString("%left a b");
        Assert.Equal(2, y2.Declares.Count);
        Assert.Equal(y2.Declares["a"], new() { Assoc = AssocTypes.Left, Name = "a", Priority = 1 }, new EqDeclarate());
        Assert.Equal(y2.Declares["b"], new() { Assoc = AssocTypes.Left, Name = "b", Priority = 1 }, new EqDeclarate());

        var y3 = RunString("%left a b %left c");
        Assert.Equal(3, y3.Declares.Count);
        Assert.Equal(y3.Declares["a"], new() { Assoc = AssocTypes.Left, Name = "a", Priority = 1 }, new EqDeclarate());
        Assert.Equal(y3.Declares["b"], new() { Assoc = AssocTypes.Left, Name = "b", Priority = 1 }, new EqDeclarate());
        Assert.Equal(y3.Declares["c"], new() { Assoc = AssocTypes.Left, Name = "c", Priority = 2 }, new EqDeclarate());
    }

    [Fact]
    public void Type()
    {
        _ = Assert.Throws<SyntaxErrorException>(() => RunString("%type a"));

        var y2 = RunString("%type<type> a b");
        Assert.Equal(2, y2.Declares.Count);
        Assert.Equal(y2.Declares["a"], new() { Assoc = AssocTypes.Type, Name = "a", Priority = 1, Type = "type" }, new EqDeclarate());
        Assert.Equal(y2.Declares["b"], new() { Assoc = AssocTypes.Type, Name = "b", Priority = 1, Type = "type" }, new EqDeclarate());

        var y3 = RunString("%type<type> a b %type<type2> c");
        Assert.Equal(3, y3.Declares.Count);
        Assert.Equal(y3.Declares["a"], new() { Assoc = AssocTypes.Type, Name = "a", Priority = 1, Type = "type" }, new EqDeclarate());
        Assert.Equal(y3.Declares["b"], new() { Assoc = AssocTypes.Type, Name = "b", Priority = 1, Type = "type" }, new EqDeclarate());
        Assert.Equal(y3.Declares["c"], new() { Assoc = AssocTypes.Type, Name = "c", Priority = 2, Type = "type2" }, new EqDeclarate());
    }

    [Fact]
    public void Char()
    {
        var y = RunString("%left 'a'");
        Assert.Single(y.Declares);
        Assert.Equal(y.Declares["'a"], new() { Assoc = AssocTypes.Left, Name = "'a", Priority = 1 }, new EqDeclarate());

        var y2 = RunString("%left 'a' a");
        Assert.Equal(2, y2.Declares.Count);
        Assert.Equal(y2.Declares["'a"], new() { Assoc = AssocTypes.Left, Name = "'a", Priority = 1 }, new EqDeclarate());
        Assert.Equal(y2.Declares["a"], new() { Assoc = AssocTypes.Left, Name = "a", Priority = 1 }, new EqDeclarate());
    }
}
