﻿using System.Collections.Generic;
using System.Text;

namespace Yanp;

public class Syntax
{
    public StringBuilder HeaderCode { get; } = new();
    public string FooterCode { get; set; } = "";
    public string Start { get; set; } = "";
    public string Default { get; set; } = "";

    public Dictionary<string, string> Defines { get; } = new();

    public Dictionary<string, Declarate> Declares { get; } = new();

    public Dictionary<string, List<GrammarLine>> Grammars { get; } = new();
}
