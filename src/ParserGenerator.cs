using Parser;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public static IEnumerable<(Token Head, List<Token> Grammars)> ParserGrammarLines(Lexer lex)
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
                    yield return (head, grams);
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
        var anonymous_action = 0;
        var create_anonymous_action = (Token t) =>
        {
            anonymous_action++;
            var name = $"{{{anonymous_action}}}";
            syntax.Grammars.Add(name, new() { new() { Action = t } });
            return new Token() { LineNumber = t.LineNumber, LineColumn = t.LineColumn, Type = Symbols.VAR, Value = name };
        };
        var register_declate = (Token t) =>
        {
            if (!syntax.Declares.ContainsKey(t.Value)) syntax.Declares.Add(t.Value, new Declarate { Assoc = AssocTypes.Type, Name = t.Value });
        };

        foreach (var g in ParserGrammarLines(lex))
        {
            var prec = g.Grammars.Where(x => x.Type == Symbols.PREC).FirstOrDefault();
            var action = g.Grammars.Count > 0 && g.Grammars.Last().Type == Symbols.ACTION ? g.Grammars.Last() : null;
            if (action is { }) _ = g.Grammars.Remove(action);

            var line = g.Grammars
                .Where(x => x.Type != Symbols.PREC)
                .Select((x, i) => x.Type == Symbols.ACTION ? create_anonymous_action(x) : x)
                .ToList();

            register_declate(g.Head);
            line.ForEach(register_declate);
            if (!syntax.Grammars.ContainsKey(g.Head.Value)) syntax.Grammars.Add(g.Head.Value, new());
            syntax.Grammars[g.Head.Value].Add(new() { Grammars = line, Prec = prec, Action = action });
        }
    }
}
