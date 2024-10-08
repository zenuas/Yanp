﻿using Mina.Extension;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yanp.Parser;

public class Lexer
{
    public required SourceCodeReader BaseReader { get; init; }
    public List<Token> Store { get; } = [];
    public static Dictionary<char, Symbols> ReservedChar { get; } = new()
        {
            { ':', Symbols.__Colon },
            { ';', Symbols.__Semicolon },
            { '|', Symbols.__VerticaLine },
        };
    public static Dictionary<string, Symbols> ReservedString { get; } = new()
        {
            { "%token", Symbols.TOKEN },
            { "%left", Symbols.LEFT },
            { "%right", Symbols.RIGHT },
            { "%nonassoc", Symbols.NONASSOC },
            { "%type", Symbols.TYPE },
            { "%default", Symbols.DEFAULT },
            { "%define", Symbols.DEFINE },
            { "%start", Symbols.START },
            { "%prec", Symbols.PREC },
        };

    public Token PeekToken()
    {
        if (Store.IsEmpty())
        {
            _ = ReadSkipWhiteSpace(BaseReader);
            Store.Add(ReadToken(BaseReader));
        }

        return Store.First();
    }

    public Token ReadToken()
    {
        var t = PeekToken();
        Store.RemoveAt(0);
        return t;
    }

    public void UnReadToken(Token t) => Store.Insert(0, t);

    public static bool ReadSkipWhiteSpace(SourceCodeReader reader)
    {
        if (reader.EndOfStream) return false;

        while (char.IsWhiteSpace(reader.PeekChar()))
        {
            _ = reader.ReadChar();

            if (reader.EndOfStream) return false;
        }
        return true;
    }

    public static Token ReadToken(SourceCodeReader reader)
    {
        if (reader.EndOfStream) return new() { Type = Symbols.__EOF, LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };

        var c = reader.PeekChar();
        if (ReservedChar.TryGetValue(c, out var value)) return new() { Type = value, LineNumber = reader.LineNumber, LineColumn = reader.LineColumn, Value = reader.ReadChar().ToString() };

        switch (c)
        {
            case '/':
                _ = reader.ReadChar();
                if (reader.PeekChar() != '/') break;
                goto case '#';

            case '#':
                _ = reader.ReadLine();
                _ = ReadSkipWhiteSpace(reader);
                return ReadToken(reader);

            case '\'':
                return ReadChar(reader);

            case '%':
                _ = reader.ReadChar();
                var next = reader.PeekChar();
                if (IsAlphabet(next))
                {
                    var t = ReadVariable(reader, "%");
                    return t.Type == Symbols.VAR
                        ? throw new SyntaxErrorException($"undefined keyword {t.Value}") { LineNumber = reader.LineNumber, LineColumn = reader.LineColumn }
                        : t;
                }
                else if (next == '%')
                {
                    _ = reader.ReadChar();
                    return new() { Type = Symbols.PartEnd, LineNumber = reader.LineNumber, LineColumn = reader.LineColumn - 1 };
                }
                else if (next == '{')
                {
                    _ = reader.ReadChar();
                    return new() { Type = Symbols.InlineBlock, LineNumber = reader.LineNumber, LineColumn = reader.LineColumn - 2, Value = ReadText(reader, "%}") };
                }
                break;

            case '<':
                return new() { Type = Symbols.DECLARE, LineNumber = reader.LineNumber, LineColumn = reader.LineColumn, Value = ReadTypeDeclare(reader) };

            case '{':
                return new() { Type = Symbols.ACTION, LineNumber = reader.LineNumber, LineColumn = reader.LineColumn, Value = ReadAction(reader) };

            default:
                if (IsAlphabet(c)) return ReadVariable(reader);
                break;
        }

        throw new SyntaxErrorException("syntax error") { LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };
    }

    public static string ReadText(SourceCodeReader reader, string eofmark = "")
    {
        var text = new StringBuilder();
        if (!reader.EndOfStream)
        {
            var first = reader.ReadLine();
            if (first != "") _ = text.AppendLine(first);
        }

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (eofmark != "" && line == eofmark) return text.ToString();
            _ = text.AppendLine(line);
        }

        return eofmark != ""
            ? throw new SyntaxErrorException("missing eof mark") { LineNumber = reader.LineNumber, LineColumn = reader.LineColumn }
            : text.ToString();
    }

    public static string ReadAction(SourceCodeReader reader)
    {
        if (reader.ReadChar() != '{') throw new SyntaxErrorException("syntax error") { LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };

        var text = new StringBuilder();
        var nest = 1;
        char? quot = null;
        while (!reader.EndOfStream)
        {
            var c = reader.ReadChar();

            if (quot is { } q)
            {
                _ = text.Append(c);
                if (q == c)
                {
                    quot = null;
                }
                else if (c == '\\' && q == reader.PeekChar())
                {
                    _ = reader.ReadChar();
                    _ = text.Append(q);
                }
                continue;
            }

            switch (c)
            {
                case '{':
                    nest++;
                    _ = text.Append(c);
                    break;

                case '}':
                    nest--;
                    if (nest <= 0) return text.ToString();
                    _ = text.Append(c);
                    break;

                case '"':
                case '\'':
                    _ = text.Append(c);
                    quot ??= c;
                    break;

                default:
                    _ = text.Append(c);
                    break;
            }
        }

        throw new SyntaxErrorException("action read EOF") { LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };
    }

    public static Token ReadVariable(SourceCodeReader reader, string prefix = "")
    {
        var line = reader.LineNumber;
        var col = reader.LineColumn - prefix.Length;
        var s = new StringBuilder(prefix);

        while (IsWord(reader.PeekChar())) _ = s.Append(reader.ReadChar());
        var name = s.ToString();
        return ReservedString.TryGetValue(name, out var value)
            ? new() { Type = value, LineNumber = line, LineColumn = col, Value = name }
            : new() { Type = Symbols.VAR, LineNumber = line, LineColumn = col, Value = name };
    }

    public static string ReadTypeDeclare(SourceCodeReader reader)
    {
        var line = reader.LineNumber;
        var col = reader.LineColumn;
        var s = new StringBuilder();

        if (reader.EndOfStream || reader.ReadChar() != '<') throw new SyntaxErrorException("syntax error") { LineNumber = line, LineColumn = col };
        var nest = 1;
        while (!reader.EndOfStream)
        {
            var c = reader.ReadChar();
            if (c == '<')
            {
                nest++;
            }
            else if (c == '>')
            {
                nest--;
                if (nest <= 0) break;
            }
            _ = s.Append(c);
        }
        return nest > 0 || s.Length == 0
            ? throw new SyntaxErrorException("syntax error") { LineNumber = line, LineColumn = col }
            : s.ToString();
    }

    public static Token ReadChar(SourceCodeReader reader)
    {
        var line = reader.LineNumber;
        var col = reader.LineColumn;

        if (reader.EndOfStream || reader.ReadChar() != '\'') throw new SyntaxErrorException("syntax error") { LineNumber = line, LineColumn = col };
        if (reader.EndOfStream) throw new SyntaxErrorException("syntax error") { LineNumber = line, LineColumn = col };

        var c = reader.ReadChar();
        if (c == '\'')
        {
            if (reader.EndOfStream) throw new SyntaxErrorException("syntax error") { LineNumber = line, LineColumn = col };

            var next = reader.ReadChar();
            c = next switch
            {
                '\'' or '"' or '\\' => next,
                't' => '\t',
                'r' => '\r',
                'n' => '\n',
                _ => throw new SyntaxErrorException($"parse escape char error ({next})") { LineNumber = line, LineColumn = col },
            };
        }

        return reader.EndOfStream || reader.ReadChar() != '\''
            ? throw new SyntaxErrorException("syntax error") { LineNumber = line, LineColumn = col }
            : new() { Type = Symbols.CHAR, LineNumber = line, LineColumn = col, Value = $"'{c}'" };
    }

    public static bool IsNumber(char c) => c >= '0' && c <= '9';

    public static bool IsNoneZeroNumber(char c) => c >= '1' && c <= '9';

    public static bool IsBinary(char c) => c == '0' || c == '1';

    public static bool IsOctal(char c) => c >= '0' && c <= '7';

    public static bool IsHexadecimal(char c) => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

    public static bool IsFloatingNumber(char c) => c == '.' || IsNumber(c);

    public static bool IsLowerAlphabet(char c) => c >= 'a' && c <= 'z';

    public static bool IsUpperAlphabet(char c) => c >= 'A' && c <= 'Z';

    public static bool IsAlphabet(char c) => IsLowerAlphabet(c) || IsUpperAlphabet(c);

    public static bool IsWord(char c) => c == '_' || IsLowerAlphabet(c) || IsUpperAlphabet(c) || IsNumber(c);
}
