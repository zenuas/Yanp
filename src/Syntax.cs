using System.Collections.Generic;
using System.Text;

namespace Yanp;

public class Syntax
{
    public StringBuilder HeaderCode { get; } = new();
    public StringBuilder FooterCode { get; } = new();
    public string Start { get; set; } = "";
    public string Default { get; set; } = "";

    public Dictionary<string, Declarate> Declares { get; } = new();
}
