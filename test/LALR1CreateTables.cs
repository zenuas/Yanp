using Extensions;
using Xunit;
using Yanp.TemplateEngine;

namespace Yanp.Test;

public class LALR1CreateTables
{
    private static string RunString(string text)
    {
        var syntax = SyntaxParser.Parse(new StringReader(text));
        var nodes = LR0.Generate(syntax);
        LALR1.Generate(syntax, nodes);
        var tables = nodes.Select((x, i) => LALR1.CreateTables(syntax, x, i)).ToArray();

        return "\r\n" + tables.Select(table =>
        {
            var conflicts = table.Conflicts.Join("\r\n");
            var lines = table.Node.LinesToString().Select(x => $"\t{x}\r\n").Join("");
            var shifts = table.Node.Nexts.Select(x => $"\t{x.Name}  shift {tables.First(y => y.Node.Equals(x)).Index}\r\n").Join("");

            return $"{conflicts}{(conflicts.Length > 0 ? "\r\n" : "")}state {table.Index}\r\n{lines}\r\n{shifts}{(shifts.Length > 0 ? "\r\n" : "")}";
        }).Join("");
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
    public void Expr1()
    {
        var verbose = RunString(@"
%%
expr : expr '+' expr
     | NUM
");
        Assert.Equal(@"
state 0
	$ACCEPT : . expr $END
	expr : . expr '+' expr
	expr : . NUM

	expr  shift 1
	NUM  shift 5

state 1
	$ACCEPT : expr . $END
	expr : expr . '+' expr

	$END  shift 2
	'+'  shift 3

state 2
	$ACCEPT : expr $END .

state 3
	expr : expr '+' . expr
	expr : . expr '+' expr
	expr : . NUM

	expr  shift 4
	NUM  shift 5

shift/reduce conflict ([shift] '+', [reduce] expr)
state 4
	expr : expr '+' expr . [$END]
	expr : expr . '+' expr

	'+'  shift 3

state 5
	expr : NUM . ['+', $END]

", verbose);
    }

    [Fact]
    public void Expr2()
    {
        var verbose = RunString(@"
%left '+'
%%
expr : expr '+' expr
     | NUM
");
        Assert.Equal(@"
state 0
	$ACCEPT : . expr $END
	expr : . expr '+' expr
	expr : . NUM

	expr  shift 1
	NUM  shift 5

state 1
	$ACCEPT : expr . $END
	expr : expr . '+' expr

	$END  shift 2
	'+'  shift 3

state 2
	$ACCEPT : expr $END .

state 3
	expr : expr '+' . expr
	expr : . expr '+' expr
	expr : . NUM

	expr  shift 4
	NUM  shift 5

state 4
	expr : expr '+' expr . ['+', $END]
	expr : expr . '+' expr

state 5
	expr : NUM . ['+', $END]

", verbose);
    }

    [Fact]
    public void Expr3()
    {
        var verbose = RunString(@"
%right '+'
%%
expr : expr '+' expr
     | NUM
");
        Assert.Equal(@"
state 0
	$ACCEPT : . expr $END
	expr : . expr '+' expr
	expr : . NUM

	expr  shift 1
	NUM  shift 5

state 1
	$ACCEPT : expr . $END
	expr : expr . '+' expr

	$END  shift 2
	'+'  shift 3

state 2
	$ACCEPT : expr $END .

state 3
	expr : expr '+' . expr
	expr : . expr '+' expr
	expr : . NUM

	expr  shift 4
	NUM  shift 5

state 4
	expr : expr '+' expr . [$END]
	expr : expr . '+' expr

	'+'  shift 3

state 5
	expr : NUM . ['+', $END]

", verbose);
    }

    [Fact]
    public void Expr4()
    {
        var verbose = RunString(@"
%left '+'
%left '*'
%%
expr : expr '+' expr
     | expr '*' expr
     | NUM
");
        Assert.Equal(@"
state 0
	$ACCEPT : . expr $END
	expr : . expr '+' expr
	expr : . expr '*' expr
	expr : . NUM

	expr  shift 1
	NUM  shift 7

state 1
	$ACCEPT : expr . $END
	expr : expr . '+' expr
	expr : expr . '*' expr

	$END  shift 2
	'+'  shift 3
	'*'  shift 5

state 2
	$ACCEPT : expr $END .

state 3
	expr : expr '+' . expr
	expr : . expr '+' expr
	expr : . expr '*' expr
	expr : . NUM

	expr  shift 4
	NUM  shift 7

state 4
	expr : expr '+' expr . ['+', $END]
	expr : expr . '+' expr
	expr : expr . '*' expr

	'*'  shift 5

state 5
	expr : expr '*' . expr
	expr : . expr '+' expr
	expr : . expr '*' expr
	expr : . NUM

	expr  shift 6
	NUM  shift 7

state 6
	expr : expr '*' expr . ['*', '+', $END]
	expr : expr . '+' expr
	expr : expr . '*' expr

state 7
	expr : NUM . ['*', '+', $END]

", verbose);
    }

    [Fact]
    public void Expr5()
    {
        var verbose = RunString(@"
%left '+' '-'
%left '*' '/'
%%
expr : expr '+' expr
     | expr '-' expr
     | expr '*' expr
     | expr '/' expr
     | NUM
");
        Assert.Equal(@"
state 0
	$ACCEPT : . expr $END
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . expr '*' expr
	expr : . expr '/' expr
	expr : . NUM

	expr  shift 1
	NUM  shift 11

state 1
	$ACCEPT : expr . $END
	expr : expr . '+' expr
	expr : expr . '-' expr
	expr : expr . '*' expr
	expr : expr . '/' expr

	$END  shift 2
	'+'  shift 3
	'-'  shift 5
	'*'  shift 7
	'/'  shift 9

state 2
	$ACCEPT : expr $END .

state 3
	expr : expr '+' . expr
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . expr '*' expr
	expr : . expr '/' expr
	expr : . NUM

	expr  shift 4
	NUM  shift 11

state 4
	expr : expr '+' expr . ['-', '+', $END]
	expr : expr . '+' expr
	expr : expr . '-' expr
	expr : expr . '*' expr
	expr : expr . '/' expr

	'*'  shift 7
	'/'  shift 9

state 5
	expr : expr '-' . expr
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . expr '*' expr
	expr : . expr '/' expr
	expr : . NUM

	expr  shift 6
	NUM  shift 11

state 6
	expr : expr '-' expr . ['-', '+', $END]
	expr : expr . '+' expr
	expr : expr . '-' expr
	expr : expr . '*' expr
	expr : expr . '/' expr

	'*'  shift 7
	'/'  shift 9

state 7
	expr : expr '*' . expr
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . expr '*' expr
	expr : . expr '/' expr
	expr : . NUM

	expr  shift 8
	NUM  shift 11

state 8
	expr : expr '*' expr . ['-', '*', '/', '+', $END]
	expr : expr . '+' expr
	expr : expr . '-' expr
	expr : expr . '*' expr
	expr : expr . '/' expr

state 9
	expr : expr '/' . expr
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . expr '*' expr
	expr : . expr '/' expr
	expr : . NUM

	expr  shift 10
	NUM  shift 11

state 10
	expr : expr '/' expr . ['-', '*', '/', '+', $END]
	expr : expr . '+' expr
	expr : expr . '-' expr
	expr : expr . '*' expr
	expr : expr . '/' expr

state 11
	expr : NUM . ['-', '*', '/', '+', $END]

", verbose);
    }

    [Fact]
    public void Expr6()
    {
        var verbose = RunString(@"
%left '+' '-'
%left '*' '/'
%right '^'
%%
expr : expr '+' expr
     | expr '-' expr
     | expr '*' expr
     | expr '/' expr
     | expr '^' expr
     | NUM
");
        Assert.Equal(@"
state 0
	$ACCEPT : . expr $END
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . expr '*' expr
	expr : . expr '/' expr
	expr : . expr '^' expr
	expr : . NUM

	expr  shift 1
	NUM  shift 13

state 1
	$ACCEPT : expr . $END
	expr : expr . '+' expr
	expr : expr . '-' expr
	expr : expr . '*' expr
	expr : expr . '/' expr
	expr : expr . '^' expr

	$END  shift 2
	'+'  shift 3
	'-'  shift 5
	'*'  shift 7
	'/'  shift 9
	'^'  shift 11

state 2
	$ACCEPT : expr $END .

state 3
	expr : expr '+' . expr
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . expr '*' expr
	expr : . expr '/' expr
	expr : . expr '^' expr
	expr : . NUM

	expr  shift 4
	NUM  shift 13

state 4
	expr : expr '+' expr . ['-', '+', $END]
	expr : expr . '+' expr
	expr : expr . '-' expr
	expr : expr . '*' expr
	expr : expr . '/' expr
	expr : expr . '^' expr

	'*'  shift 7
	'/'  shift 9
	'^'  shift 11

state 5
	expr : expr '-' . expr
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . expr '*' expr
	expr : . expr '/' expr
	expr : . expr '^' expr
	expr : . NUM

	expr  shift 6
	NUM  shift 13

state 6
	expr : expr '-' expr . ['-', '+', $END]
	expr : expr . '+' expr
	expr : expr . '-' expr
	expr : expr . '*' expr
	expr : expr . '/' expr
	expr : expr . '^' expr

	'*'  shift 7
	'/'  shift 9
	'^'  shift 11

state 7
	expr : expr '*' . expr
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . expr '*' expr
	expr : . expr '/' expr
	expr : . expr '^' expr
	expr : . NUM

	expr  shift 8
	NUM  shift 13

state 8
	expr : expr '*' expr . ['-', '*', '/', '+', $END]
	expr : expr . '+' expr
	expr : expr . '-' expr
	expr : expr . '*' expr
	expr : expr . '/' expr
	expr : expr . '^' expr

	'^'  shift 11

state 9
	expr : expr '/' . expr
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . expr '*' expr
	expr : . expr '/' expr
	expr : . expr '^' expr
	expr : . NUM

	expr  shift 10
	NUM  shift 13

state 10
	expr : expr '/' expr . ['-', '*', '/', '+', $END]
	expr : expr . '+' expr
	expr : expr . '-' expr
	expr : expr . '*' expr
	expr : expr . '/' expr
	expr : expr . '^' expr

	'^'  shift 11

state 11
	expr : expr '^' . expr
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . expr '*' expr
	expr : . expr '/' expr
	expr : . expr '^' expr
	expr : . NUM

	expr  shift 12
	NUM  shift 13

state 12
	expr : expr '^' expr . ['-', '*', '/', '+', $END]
	expr : expr . '+' expr
	expr : expr . '-' expr
	expr : expr . '*' expr
	expr : expr . '/' expr
	expr : expr . '^' expr

	'^'  shift 11

state 13
	expr : NUM . ['-', '*', '/', '^', '+', $END]

", verbose);
    }

    [Fact]
    public void Nonassoc1()
    {
        var verbose = RunString(@"
%nonassoc '+'
%nonassoc '-'
%%
expr : expr '+' expr
     | expr '-' expr
     | NUM
");
        Assert.Equal(@"
state 0
	$ACCEPT : . expr $END
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . NUM

	expr  shift 1
	NUM  shift 7

state 1
	$ACCEPT : expr . $END
	expr : expr . '+' expr
	expr : expr . '-' expr

	$END  shift 2
	'+'  shift 3
	'-'  shift 5

state 2
	$ACCEPT : expr $END .

state 3
	expr : expr '+' . expr
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . NUM

	expr  shift 4
	NUM  shift 7

nonassociative ([shift] '+', [reduce] expr)
state 4
	expr : expr '+' expr . [$END]
	expr : expr . '+' expr
	expr : expr . '-' expr

	'+'  shift 3
	'-'  shift 5

state 5
	expr : expr '-' . expr
	expr : . expr '+' expr
	expr : . expr '-' expr
	expr : . NUM

	expr  shift 6
	NUM  shift 7

nonassociative ([shift] '-', [reduce] expr)
state 6
	expr : expr '-' expr . ['+', $END]
	expr : expr . '+' expr
	expr : expr . '-' expr

	'-'  shift 5

state 7
	expr : NUM . ['-', '+', $END]

", verbose);
    }
}
