using Xunit;

namespace Yanp.Test;

public class LALR1CreateTables
{
    private static string RunString(string text)
    {
        var syntax = SyntaxParser.Parse(new StringReader(text));
        var nodes = LR0.Generate(syntax);
        LALR1.Generate(syntax, nodes);
        var tables = LALR1.CreateTables(syntax, nodes);

        var source = @"
@using System.Linq
@foreach (var table in Model.Tables)
{
	if (table.get_Conflicts().Length > 0)
	{
@:@string.Join(""\r\n"", table.get_Conflicts())
	}
@:state @table.get_Index()
	foreach (var line in table.get_Node().get_Lines())
	{
	@:@line
	}
@:
	var shift_found = false;
	foreach (var next in table.get_Node().Nexts)
	{
		shift_found = true;
	@:@next.get_Name()  shift @Model.NodeToTable(next).get_Index()
	}
	if (shift_found)
	{
@:
	}
}
";
        using var output = new StringWriter();
        TemplateEngine.Engine.Run(syntax, nodes, tables, source, output);
        return output.ToString();
    }

    [Fact]
    public void DanglingElse1()
    {
        var verbose = RunString(@"
%%
stmt : if
     | '1'
if : IF expr stmt
   | IF expr stmt ELSE stmt
expr : '2'
");
        Assert.Equal(@"
state 0
	$ACCEPT : . stmt $END
	stmt : . if
	if : . IF expr stmt
	if : . IF expr stmt ELSE stmt
	stmt : . '1'

	stmt  shift 1
	if  shift 3
	IF  shift 4
	'1'  shift 7

state 1
	$ACCEPT : stmt . $END

	$END  shift 2

state 2
	$ACCEPT : stmt $END .

state 3
	stmt : if . [$END, ELSE]

state 4
	if : IF . expr stmt
	expr : . '2'
	if : IF . expr stmt ELSE stmt

	'2'  shift 5
	expr  shift 6

state 5
	expr : '2' . ['1', IF]

state 6
	if : IF expr . stmt
	stmt : . if
	if : . IF expr stmt
	if : . IF expr stmt ELSE stmt
	stmt : . '1'
	if : IF expr . stmt ELSE stmt

	if  shift 3
	IF  shift 4
	'1'  shift 7
	stmt  shift 8

state 7
	stmt : '1' . [$END, ELSE]

shift/reduce conflict ([shift] ELSE, [reduce] if)
state 8
	if : IF expr stmt . [$END]
	if : IF expr stmt . ELSE stmt

	ELSE  shift 9

state 9
	if : IF expr stmt ELSE . stmt
	stmt : . if
	if : . IF expr stmt
	if : . IF expr stmt ELSE stmt
	stmt : . '1'

	stmt  shift 10
	if  shift 3
	IF  shift 4
	'1'  shift 7

state 10
	if : IF expr stmt ELSE stmt . [$END, ELSE]

", verbose);
    }

    [Fact]
    public void DanglingElse2()
    {
        var verbose = RunString(@"
%left ELSE
%%
stmt : if
     | '1'
if : IF expr stmt
   | IF expr stmt ELSE stmt
expr : '2'
");
        Assert.Equal(@"
state 0
	$ACCEPT : . stmt $END
	stmt : . if
	if : . IF expr stmt
	if : . IF expr stmt ELSE stmt
	stmt : . '1'

	stmt  shift 1
	if  shift 3
	IF  shift 4
	'1'  shift 7

state 1
	$ACCEPT : stmt . $END

	$END  shift 2

state 2
	$ACCEPT : stmt $END .

state 3
	stmt : if . [$END, ELSE]

state 4
	if : IF . expr stmt
	expr : . '2'
	if : IF . expr stmt ELSE stmt

	'2'  shift 5
	expr  shift 6

state 5
	expr : '2' . ['1', IF]

state 6
	if : IF expr . stmt
	stmt : . if
	if : . IF expr stmt
	if : . IF expr stmt ELSE stmt
	stmt : . '1'
	if : IF expr . stmt ELSE stmt

	if  shift 3
	IF  shift 4
	'1'  shift 7
	stmt  shift 8

state 7
	stmt : '1' . [$END, ELSE]

state 8
	if : IF expr stmt . [$END]
	if : IF expr stmt . ELSE stmt

	ELSE  shift 9

state 9
	if : IF expr stmt ELSE . stmt
	stmt : . if
	if : . IF expr stmt
	if : . IF expr stmt ELSE stmt
	stmt : . '1'

	stmt  shift 10
	if  shift 3
	IF  shift 4
	'1'  shift 7

state 10
	if : IF expr stmt ELSE stmt . [$END, ELSE]

", verbose);
    }

    [Fact]
    public void DanglingElse3()
    {
        var verbose = RunString(@"
%right ELSE
%%
stmt : if
     | '1'
if : IF expr stmt
   | IF expr stmt ELSE stmt
expr : '2'
");
        Assert.Equal(@"
state 0
	$ACCEPT : . stmt $END
	stmt : . if
	if : . IF expr stmt
	if : . IF expr stmt ELSE stmt
	stmt : . '1'

	stmt  shift 1
	if  shift 3
	IF  shift 4
	'1'  shift 7

state 1
	$ACCEPT : stmt . $END

	$END  shift 2

state 2
	$ACCEPT : stmt $END .

state 3
	stmt : if . [$END, ELSE]

state 4
	if : IF . expr stmt
	expr : . '2'
	if : IF . expr stmt ELSE stmt

	'2'  shift 5
	expr  shift 6

state 5
	expr : '2' . ['1', IF]

state 6
	if : IF expr . stmt
	stmt : . if
	if : . IF expr stmt
	if : . IF expr stmt ELSE stmt
	stmt : . '1'
	if : IF expr . stmt ELSE stmt

	if  shift 3
	IF  shift 4
	'1'  shift 7
	stmt  shift 8

state 7
	stmt : '1' . [$END, ELSE]

state 8
	if : IF expr stmt . [$END, ELSE]
	if : IF expr stmt . ELSE stmt

state 9
	if : IF expr stmt ELSE . stmt
	stmt : . if
	if : . IF expr stmt
	if : . IF expr stmt ELSE stmt
	stmt : . '1'

	stmt  shift 10
	if  shift 3
	IF  shift 4
	'1'  shift 7

state 10
	if : IF expr stmt ELSE stmt . [$END, ELSE]

", verbose);
    }
}
