﻿using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser;

public class Lexer
{
    public SourceCodeReader BaseReader { get; }
    //public Parser? Parser { get; init; }
    public List<Token> Store { get; } = new List<Token>();
    public static Dictionary<char, Symbols> ReservedChar { get; } = new Dictionary<char, Symbols>
            {
                { ':', Symbols.__Colon },
            };
    public static Dictionary<string, Symbols> ReservedString { get; } = new Dictionary<string, Symbols>
            {
                { "%start", Symbols.LET },
                { "%left", Symbols.STRUCT },
                { "%right", Symbols.CLASS },
                { "instance", Symbols.INSTANCE },
                { "sub", Symbols.SUB },
                { "if", Symbols.IF },
                { "then", Symbols.THEN },
                { "else", Symbols.ELSE },
                { "switch", Symbols.SWITCH },
                { "true", Symbols.TRUE },
                { "false", Symbols.FALSE },
                { "null", Symbols.NULL },
                { "is", Symbols.IS },
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
        //if (Parser is { })
        //{
        //}
        return first;
    }

    public Token ReadToken()
    {
        var t = PeekToken();
        Store.RemoveAt(0);
        return t;
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
        if (reader.EndOfStream) return new Token { Type = Symbols.EOL, LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };

        var c = reader.PeekChar();
        if (ReservedChar.ContainsKey(c)) return new Token { Type = ReservedChar[c], LineNumber = reader.LineNumber, LineColumn = reader.LineColumn, Value = reader.ReadChar().ToString() };

        switch (c)
        {
            case '"':
                return ReadString(reader);

            case '%':
                _ = reader.ReadChar();
                if (IsAlphabet(reader.PeekChar())) return ReadVariable(reader, "%");
                break;

            default:
                if (IsAlphabet(c)) return ReadVariable(reader);
                break;
        }

        throw new SyntaxErrorException("syntax error") { LineNumber = reader.LineNumber, LineColumn = reader.LineColumn };
    }

    public static Token ReadVariable(SourceCodeReader reader) => ReadVariable(reader, "");

    public static Token ReadVariable(SourceCodeReader reader, string prefix)
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

    public static Token ReadString(SourceCodeReader reader)
    {
        var line = reader.LineNumber;
        var col = reader.LineColumn;

        var start = reader.ReadChar();
        var s = new StringBuilder();
        while (!reader.EndOfStream)
        {
            var c = reader.ReadChar();
            if (c == start) break;
            _ = s.Append(c);
        }
        return new Token { Type = Symbols.STR, LineNumber = line, LineColumn = col, Value = s.ToString() };
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
