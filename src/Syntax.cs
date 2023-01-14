using System.Text;

public class Syntax
{
    public StringBuilder HeaderCode { get; } = new StringBuilder();
    public StringBuilder FooterCode { get; } = new StringBuilder();
    public string Start { get; set; } = "";
    public string Default { get; set; } = "";
}