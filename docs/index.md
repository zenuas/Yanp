## パーサジェネレータについて

パーサジェネレータは文法ファイルを元に構文解析器を作成するプログラムである。
本章では**パーサジェネレータの作り方**を解説する。
なお、[Yanp](https://github.com/zenuas/Yanp)を元に解説するためここではLALR(1)の構文解析器に限定して解説する。

[パーサジェネレータ/Wikipedia](https://ja.wikipedia.org/wiki/%E3%83%91%E3%83%BC%E3%82%B5%E3%82%B8%E3%82%A7%E3%83%8D%E3%83%AC%E3%83%BC%E3%82%BF)

### はじめに

パーサジェネレータの説明に当たり、各章の説明では構文規則に次の前提を設ける。

* 原則構文規則のみを記述する
* 英大文字の構文規則は終端記号とする (例: `IF`、`NUM`)
* 英子文字の構文規則は非終端記号とする (例: `if`、`num`)
* 非終端記号の構文規則の記載がない場合は同名の終端記号が設定されているものとする (例: `if : IF`)

### 構文規則の読み方

構文規則の間に`.`が入れば、そこを現在の位置とする。
記号を読み進めると`.`の位置が右に移動し、末尾まで到達すると還元する。
規則が並んでいる場合は複数の候補がある事を示す。

```
start : . FIRST SECOND THIRD
start : . OTHER

↓ FIRSTを読み込み後

start : FIRST . SECOND THIRD

↓ SECONDを読み込み後

start : FIRST SECOND . THIRD

↓ THIRDを読み込み後

start : FIRST SECOND THIRD .
```

還元する行の末尾に`[記号1, 記号2, ...]`とあるのは先読み記号である。
次の記号が先読み記号に合致する場合のみ還元される。

```
if : IF cond THEN expr . [$END]
if : IF cond THEN expr . ELSE expr
```