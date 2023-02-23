# Yanp [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE)

Yanp is parser generator  

## Description

Yanpはパーサジェネレータである  
LALR(1)のパーサを作成する  

## Usage

```
yanp [-iv] [-o dir] filename [-t template] [-l 0-2]
```

Yanpはfilenameの文法仕様を元にLALR(1)解析テーブルとパーサを作成する  
YanpはC#のパーサを出力する  

* **-i** filename  
  文法ファイルを指定する  

* **-o** dir  
  指定のディレクトリにパーサを出力する  

* **-v**  
  -vを指定するとパーサの詳細をverbose.v.txtへ出力する  

* **-t** template  
  出力パーサのテンプレートを指定する  
  C#(template.cs)、デバッグ出力(template.debug)のみ対応  

* **-l** 0-2  
  出力パーサに含まれる生成物の種類を数字で指定する  
  0 ... 最低限のパーサ、定義ファイル  
  1 ... サンプルのレキシカルアナライザや任意ライブラリ  
  2 ... 単体動作可能なサンプル  

## Demo

四則演算を行う電卓パーサ  
```
%default int

%left '+' '-'
%left '*' '/' '%'
%right '^'

%%

expr : expr '+' expr {$$ = $1 + $3; Console.WriteLine($"{$1} + {$3}");}
     | expr '-' expr {$$ = $1 - $3; Console.WriteLine($"{$1} - {$3}");}
     | expr '*' expr {$$ = $1 * $3; Console.WriteLine($"{$1} * {$3}");}
     | expr '/' expr {$$ = $1 / $3; Console.WriteLine($"{$1} / {$3}");}
     | expr '^' expr {$$ = (int)Math.Pow($1, $3); Console.WriteLine($"{$1} ^ {$3}");}
     | expr '%' expr {$$ = $1 % $3; Console.WriteLine($"{$1} % {$3}");}
     | num
num : '0' {$$ = 0;}
    | '1' {$$ = 1;}
    | '2' {$$ = 2;}
    | '3' {$$ = 3;}
    | '4' {$$ = 4;}
    | '5' {$$ = 5;}
    | '6' {$$ = 6;}
    | '7' {$$ = 7;}
    | '8' {$$ = 8;}
    | '9' {$$ = 9;}
```

## Installation

* Windows/bin: [[exe](https://github.com/zenuas/Yanp/releases)]  

## Requirement

* .NET 7  
* RazorEngine.NetCore  
  https://www.nuget.org/packages/RazorEngine.NetCore  

## Author

[zenuas@GitHub](https://github.com/zenuas)  
