﻿using Yanp.Parser;
using System;
using System.Collections.Generic;
using Yanp.Data;

namespace Yanp;

public class Model
{
    public required Syntax Syntax;
    public required Node[] Nodes;
    public required Table[] Tables;
    public required Func<string, string, string> GetDefine;
    public required Func<Token[]> GetSymbols;
    public required Func<GrammarLine[]> GetGrammarLines;
    public required Func<Node, Table> NodeToTable;
    public Dictionary<string, object> KeyValues = [];
}
