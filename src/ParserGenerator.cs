using Parser;
using System.Collections.Generic;

namespace Yanp;

public class ParserGenerator
{
    public static void ParseDeclaration(Syntax syntax, Lexer lex)
    {
        var assoc_conv = new Dictionary<Symbols, AssocTypes>() {
            { Symbols.TOKEN, AssocTypes.Type },
            { Symbols.LEFT, AssocTypes.Left },
            { Symbols.RIGHT, AssocTypes.Right },
            { Symbols.NONASSOC, AssocTypes.Nonassoc },
            { Symbols.TYPE, AssocTypes.Type },
        };
        var priority = 0;

        var read_assoc = (AssocTypes assoc, int pri) =>
        {
            if (lex.PeekToken().Type == Symbols.__EOF) throw new SyntaxErrorException($"assoc {assoc} with token") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };

            var type = lex.PeekToken().Type == Symbols.DECLARE ? lex.ReadToken().Value : "";

            if (lex.PeekToken().Type != Symbols.VAR && lex.PeekToken().Type != Symbols.CHAR) throw new SyntaxErrorException($"assoc {assoc} with token") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };
            do
            {
                var t = lex.ReadToken();
                syntax.Declares.Add(t.Value, new Declarate { Assoc = assoc, Name = t.Value, Priority = pri, Type = type });
            } while (lex.PeekToken().Type == Symbols.VAR || lex.PeekToken().Type == Symbols.CHAR);
        };

        while (lex.PeekToken().Type != Symbols.__EOF)
        {
            var t = lex.ReadToken();
            switch (t.Type)
            {
                case Symbols.TYPE:
                    if (lex.PeekToken().Type != Symbols.DECLARE) throw new SyntaxErrorException($"assoc {t.Type} with <decla-type>") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };
                    goto case Symbols.TOKEN;

                case Symbols.TOKEN:
                case Symbols.LEFT:
                case Symbols.RIGHT:
                case Symbols.NONASSOC:
                    read_assoc(assoc_conv[t.Type], ++priority);
                    break;

                case Symbols.PartEnd:
                    return;
            }
        }
    }

    public static IEnumerable<(Token Head, Token[] Grammars)> ParserGrammarLines(Lexer lex)
    {
        if (lex.PeekToken().Type == Symbols.__EOF || lex.PeekToken().Type == Symbols.PartEnd) yield break;

        Token? prev = null;
        while (true)
        {
            var head = lex.PeekToken();
            switch (head.Type)
            {
                case Symbols.__VerticaLine:
                    if (prev is null) throw new SyntaxErrorException($"bad sequence grammar token") { LineNumber = head.LineNumber, LineColumn = head.LineColumn };
                    head = prev;
                    goto GRAMMAR_LINE_;

                case Symbols.VAR:
                    _ = lex.ReadToken();
                    if (lex.PeekToken().Type != Symbols.__Colon) throw new SyntaxErrorException($"bad sequence grammar token") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };
                    _ = lex.ReadToken();

                GRAMMAR_LINE_:
                    var grams = new List<Token>();
                    while (true)
                    {
                        var g = lex.PeekToken();
                        if (g.Type == Symbols.VAR)
                        {
                            _ = lex.ReadToken();
                            if (lex.PeekToken().Type == Symbols.__Colon)
                            {
                                lex.UnReadToken(g); // head : a b g . : c
                                break;
                            }
                            grams.Add(g);
                        }
                        else if (
                            g.Type == Symbols.CHAR ||
                            g.Type == Symbols.ACTION)
                        {
                            grams.Add(lex.ReadToken());
                        }
                        else if (g.Type == Symbols.PREC)
                        {
                            _ = lex.ReadToken();
                            var prec_g = lex.ReadToken();
                            if (prec_g.Type != Symbols.VAR && prec_g.Type != Symbols.CHAR) throw new SyntaxErrorException($"bad sequence prec next token") { LineNumber = prec_g.LineNumber, LineColumn = prec_g.LineColumn };
                            grams.Add(new Token() { Type = g.Type, LineNumber = prec_g.LineNumber, LineColumn = prec_g.LineColumn, Value = prec_g.Value });
                        }
                        else if (g.Type == Symbols.__VerticaLine)
                        {
                            prev = head;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    yield return (head, grams.ToArray());
                    break;

                case Symbols.__Semicolon:
                    prev = null;
                    _ = lex.ReadToken();
                    break;

                case Symbols.PartEnd:
                    _ = lex.ReadToken();
                    yield break;

                case Symbols.__EOF:
                    yield break;

                default:
                    throw new SyntaxErrorException($"bad sequence grammar token") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };

            }
        }
    }

    public static void ParseGrammar(Syntax syntax, Lexer lex)
    {
    }
}
