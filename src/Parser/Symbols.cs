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

    /// <summary>%{ ... %}</summary>
    InlineBlock,

    /// <summary>%%</summary>
    PartEnd,

    /// <summary>:</summary>
    __Colon,

    /// <summary>;</summary>
    __Semicolon,

    /// <summary>|</summary>
    __VerticaLine,

    /// <summary>VAR</summary>
    VAR,

    /// <summary>CHAR</summary>
    CHAR,

    /// <summary>DECLARE</summary>
    DECLARE,

    /// <summary>{ ... }</summary>
    ACTION,
}
