using System;
using Yanp.Data;

namespace Yanp.TemplateEngine;

public class Model
{
    public required Option Option;
    public required Syntax Syntax;
    public required Node[] Nodes;
    public required Func<string, string, string> GetDefine;
}
