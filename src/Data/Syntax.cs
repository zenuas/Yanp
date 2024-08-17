using System.Collections.Generic;
using System.Text;

namespace Yanp.Data;

public class Syntax
{
    public StringBuilder HeaderCode { get; } = new();
    public StringBuilder FooterCode { get; set; } = new();
    public string Start { get; set; } = "";
    public string Default { get; set; } = "object";

    public Dictionary<string, string> Defines { get; } = [];

    public Dictionary<string, Declarate> Declares { get; } = [];

    public Dictionary<string, List<GrammarLine>> Grammars { get; } = [];
}
