namespace Parser;

public enum Symbols
{
    /// <summary>End of File</summary>
    __EOF,

    /// <summary>%token</summary>
    TOKEN,

    /// <summary>%left</summary>
    LEFT,

    /// <summary>%right</summary>
    RIGHT,

    /// <summary>%nonassoc</summary>
    NONASSOC,

    /// <summary>%type</summary>
    TYPE,

    /// <summary>%default</summary>
    DEFAULT,

    /// <summary>%define</summary>
    DEFINE,

    /// <summary>%start</summary>
    START,

    /// <summary>%prec</summary>
    PREC,

    /// <summary>%{</summary>
    InlineStart,

    /// <summary>%%</summary>
    PartEnd,

    /// <summary>:</summary>
    __Colon,

    /// <summary>;</summary>
    __Semicolon,

    /// <summary>|</summary>
    __VerticaLine,

    /// <summary>{</summary>
    __LeftCurlyBracket,

    /// <summary>&lt;</summary>
    __LessThan,

    /// <summary>&gt;</summary>
    __GraterThan,

    /// <summary>VAR</summary>
    VAR,

    /// <summary>STR</summary>
    STR,
}
