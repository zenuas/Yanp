using Extensions;
using Parser;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yanp.Data;

namespace Yanp;

public static class SyntaxParser
{
    public static Syntax Parse(TextReader input)
    {
        var lex = new Lexer(new() { BaseReader = input });
        var syntax = new Syntax();
        ParseDeclaration(syntax, lex);
        ParseGrammar(syntax, lex);
        GrammarsToTerminalSymbol(syntax);
        if (!lex.BaseReader.EndOfStream)
        {
            var first = lex.BaseReader.ReadLine();
            if (first != "") _ = syntax.FooterCode.AppendLine(first);
        }
        _ = syntax.FooterCode.Append(input.ReadToEnd());
        return syntax;
    }

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
                    {
                        var assoc = assoc_conv[t.Type];
                        priority++;

                        if (lex.PeekToken().Type == Symbols.__EOF) throw new SyntaxErrorException($"assoc {assoc} with token") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };

                        var type = lex.PeekToken().Type == Symbols.DECLARE ? lex.ReadToken().Value : "";

                        if (lex.PeekToken().Type != Symbols.VAR && lex.PeekToken().Type != Symbols.CHAR) throw new SyntaxErrorException($"assoc {assoc} with token") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };
                        do
                        {
                            var v = lex.ReadToken();
                            syntax.Declares.Add(v.Value, new() { Assoc = assoc, Name = v, Priority = priority, Type = type });
                        } while (lex.PeekToken().Type == Symbols.VAR || lex.PeekToken().Type == Symbols.CHAR);
                    }
                    break;

                case Symbols.DEFAULT:
                    if (lex.PeekToken().Type != Symbols.VAR) throw new SyntaxErrorException("%default with token") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };
                    syntax.Default = lex.ReadToken().Value;
                    break;

                case Symbols.DEFINE:
                    if (lex.PeekToken().Type != Symbols.VAR) throw new SyntaxErrorException("%define with name") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };
                    syntax.Defines.Add(lex.ReadToken().Value, (lex.BaseReader.ReadLine() ?? "").TrimStart());
                    break;

                case Symbols.START:
                    if (lex.PeekToken().Type != Symbols.VAR) throw new SyntaxErrorException("%start with token") { LineNumber = lex.BaseReader.LineNumber, LineColumn = lex.BaseReader.LineColumn };
                    syntax.Start = lex.ReadToken().Value;
                    break;

                case Symbols.InlineBlock:
                    _ = syntax.HeaderCode.Append(t.Value);
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
                    _ = lex.ReadToken();
                    head = new Token { LineNumber = head.LineNumber, LineColumn = head.LineColumn, Type = prev.Type, Value = prev.Value };
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
                            grams.Add(new() { Type = g.Type, LineNumber = prec_g.LineNumber, LineColumn = prec_g.LineColumn, Value = prec_g.Value });
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

                case Symbols.InlineBlock:
                    prev = null;
                    _ = lex.ReadToken();
                    yield return (head, new Token[0]);
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
            var name = $"{{{++anonymous_action}}}";
            syntax.Grammars.Add(name, new()
            {
                new()
                {
                    Name = new() { Type = Symbols.VAR, LineNumber = t.LineNumber, LineColumn = t.LineColumn, Value = name },
                    LineNumber = t.LineNumber,
                    LineColumn = t.LineColumn,
                    Action = t
                }
            });
            return new Token() { LineNumber = t.LineNumber, LineColumn = t.LineColumn, Type = Symbols.VAR, Value = name };
        };
        var register_declate = (Token t) =>
        {
            if (!syntax.Declares.ContainsKey(t.Value)) syntax.Declares.Add(t.Value, new Declarate { Assoc = AssocTypes.Type, Name = t });
        };

        foreach (var g in ParserGrammarLines(lex))
        {
            if (g.Head.Type == Symbols.InlineBlock)
            {
                _ = syntax.HeaderCode.Append(g.Head.Value);
                continue;
            }
            if (syntax.Start == "")
            {
                syntax.Start = g.Head.Value;
            }

            var prec = g.Grammars.Where(x => x.Type == Symbols.PREC).FirstOrDefault() ??
                g.Grammars
                    .Where(x => syntax.Declares.ContainsKey(x.Value) && syntax.Declares[x.Value].Priority > 0)
                    .MaxBy(x => syntax.Declares[x.Value].Priority);
            var action = g.Grammars.Length > 0 && g.Grammars.Last().Type == Symbols.ACTION ? g.Grammars.Last() : null;

            var line = g.Grammars
                .Where(x => x.Type != Symbols.PREC && !(action is { } p && x == p))
                .Select((x, i) => x.Type == Symbols.ACTION ? create_anonymous_action(x) : x)
                .ToList();

            register_declate(g.Head);
            line.ForEach(register_declate);
            if (!syntax.Grammars.ContainsKey(g.Head.Value)) syntax.Grammars.Add(g.Head.Value, new());
            var d = prec is { } ? syntax.Declares[prec.Value] : syntax.Declares[g.Head.Value];
            syntax.Grammars[g.Head.Value].Add(new() { Name = g.Head, LineNumber = g.Head.LineNumber, LineColumn = g.Head.LineColumn, Grammars = line, Prec = prec, Action = action, Priority = d.Priority, Assoc = d.Assoc });
        }
    }

    public static void GrammarsToTerminalSymbol(Syntax syntax)
    {
        syntax.Declares.Values
            .Where(x => x.Name.Type == Symbols.VAR)
            .Each(x => x.IsTerminalSymbol = !syntax.Grammars.ContainsKey(x.Name.Value));
    }
}
