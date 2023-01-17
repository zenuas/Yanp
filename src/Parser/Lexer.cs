using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser;

public class Lexer
{
    public SourceCodeReader BaseReader { get; }
    public List<Token> Store { get; } = new List<Token>();
    public static Dictionary<char, Symbols> ReservedChar { get; } = new Dictionary<char, Symbols>
            {
                { ':', Symbols.__Colon },
                { ';', Symbols.__Semicolon },
                { '|', Symbols.__VerticaLine },
                { '{', Symbols.__LeftCurlyBracket },
            };
    public static Dictionary<string, Symbols> ReservedString { get; } = new Dictionary<string, Symbols>
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

    public Lexer(SourceCodeReader reader)
    {
        BaseReader = reader;
    }

    public Token PeekToken()
    {
        if (Store.IsEmpty())
        {
            _ = ReadSkipWhiteSpace(BaseReader);
            Store.Add(ReadToken(BaseReader));
        }

        var first = Store.First();
        return first;
    }

    public Token ReadToken()
    {
        var t = PeekToken();
        Store.RemoveAt(0);
        return t;
    }

    public void UnReadToken(Token t)
    {
        Store.Insert(0, t);
    }

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
        if (reader.EndOfStream) return new Token { Type = Symbols.__EOF, LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };

        var c = reader.PeekChar();
        if (ReservedChar.ContainsKey(c)) return new Token { Type = ReservedChar[c], LineNumber = reader.LineNumber, LineColumn = reader.LineColumn, Value = reader.ReadChar().ToString() };

        switch (c)
        {
            case '\'':
                return ReadChar(reader);

            case '%':
                _ = reader.ReadChar();
                var next = reader.PeekChar();
                if (IsAlphabet(next))
                {
                    var t = ReadVariable(reader, "%");
                    if (t.Type == Symbols.VAR) throw new SyntaxErrorException($"undefined keyword {t.Value}") { LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };
                    return t;
                }
                else if (next == '%')
                {
                    _ = reader.ReadChar();
                    return new Token { Type = Symbols.PartEnd, LineNumber = reader.LineNumber, LineColumn = reader.LineColumn - 1 };
                }
                else if (next == '{')
                {
                    _ = reader.ReadChar();
                    return new Token { Type = Symbols.InlineBlock, LineNumber = reader.LineNumber, LineColumn = reader.LineColumn, Value = ReadText(reader, "%}") };
                }
                break;

            case '<':
                return ReadTypeDeclare(reader);

            default:
                if (IsAlphabet(c)) return ReadVariable(reader);
                break;
        }

        throw new SyntaxErrorException("syntax error") { LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };
    }

    public static string ReadText(SourceCodeReader reader, string eofmark = "")
    {
        var text = new StringBuilder();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (eofmark != "" && line == eofmark) return text.ToString();
            text.AppendLine(line);
        }

        if (eofmark != "") throw new SyntaxErrorException("missing eof mark") { LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };
        return text.ToString();
    }

    public static Token ReadVariable(SourceCodeReader reader, string prefix = "")
    {
        var line = reader.LineNumber;
        var col = reader.LineColumn - prefix.Length;
        var s = new StringBuilder(prefix);

        while (IsWord(reader.PeekChar())) _ = s.Append(reader.ReadChar());
        var name = s.ToString();
        return ReservedString.ContainsKey(name)
            ? new Token { Type = ReservedString[name], LineNumber = line, LineColumn = col, Value = name }
            : new Token { Type = Symbols.VAR, LineNumber = line, LineColumn = col, Value = name };
    }

    public static Token ReadTypeDeclare(SourceCodeReader reader)
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
        if (nest > 0 || s.Length == 0) throw new SyntaxErrorException("syntax error") { LineNumber = line, LineColumn = col };

        return new Token { Type = Symbols.DECLARE, LineNumber = line, LineColumn = col, Value = s.ToString() };
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
            switch (next)
            {
                case '\'':
                case '"':
                case '\\':
                    c = next;
                    break;

                case 't': c = '\t'; break;
                case 'r': c = '\r'; break;
                case 'n': c = '\n'; break;

                default:
                    throw new SyntaxErrorException($"parse escape char error ({next})") { LineNumber = line, LineColumn = col };
            }
        }

        if (reader.EndOfStream || reader.ReadChar() != '\'') throw new SyntaxErrorException("syntax error") { LineNumber = line, LineColumn = col };
        return new Token { Type = Symbols.CHAR, LineNumber = line, LineColumn = col, Value = $"'{c}" };
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
