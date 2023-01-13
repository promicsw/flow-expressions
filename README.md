

# Flow Expressions

Construct, ready to run,  Parsers of any complexity using a declarative fluent syntax in C#. The system is lightweight, fast, and loosely coupled components provide complete implementation flexibility.

## Building Flow Expressions

Flow Expressions are defined by a structure of FexElements built via a Fluent API. This defines the logical flow and operations of the expression in a very readable format. Any logic than can be expressed as a Flow Expression (think flow chart) may be implemented. 

For starters only **Parsers** will be discussed in this document!

> A Flow Expression operates on a **Context**, which is any environment that manages and provides input to the Flow Expression.
>
> **Note** For a Parser, the context would be a **Scanner** that handles text scanning and provides <i>Tokens</i> to operate on.<br/> 
> A comprehensive FexScanner is provided (see scanner reference) but you can roll your own if required. 

The following example is a complete **Expression Parser**, including evaluation and error reporting:

```csharp
public static void ExpressionEval() {
    /*
     * Expression Grammar:
     * expression     => factor ( ( '-' | '+' ) factor )* ;
     * factor         => unary ( ( '/' | '*' ) unary )* ;
     * unary          => '-' unary | primary ;
     * primary        => NUMBER | "(" expression ")" ;
    */

    // Stack and stack operator to evaluate the expression
    Stack<double> numStack = new Stack<double>()
    void Calc(Func<double, double, double> op) {
        double num2 = numStack.Pop(), num1 = numStack.Pop();
        numStack.Push(op(num1, num2));
    
    var expr1 = "9 - ( 5.5 + 3 ) * 6 - 4 / ( 9 - 1 )"
    Console.WriteLine($"Evaluating expression: {expr1}")

    var parseError = new ScanErrorLog();
    var scn = new FexScanner(expr1, parseError);
    var fex = new FlowExpression<FexScanner>();

    // Expression productions:
    var expr = fex.Seq(s => s.RefName("expr")
        .Ref("factor")
        .RepOneOf(0, -1, r => r
            .Seq(s => s.Ch('+').Ref("factor").Act(c => Calc((n1, n2) => n1 + n2)))
            .Seq(s => s.Ch('-').Ref("factor").Act(c => Calc((n1, n2) => n1 - n2)))
         ));

    fex.Seq(s => s.RefName("factor")
        .Ref("unary")
        .RepOneOf(0, -1, r => r
            .Seq(s => s.Ch('*').Ref("unary").Act(c => Calc((n1, n2) => n1 * n2)))
            .Seq(s => s.Ch('/').Ref("unary")
                       .Assert(c => numStack.Peek() != 0, e => e.LogError("Division by 0")) // Trap division by 0
                       .Act(c => Calc((n1, n2) => n1 / n2)))
         ));

    fex.Seq(s => s.RefName("unary")
        .OneOf(o => o
            .Seq(s => s.Ch('-').Ref("unary").Act(a => numStack.Push(-numStack.Pop())))
            .Ref("primary")
         ).OnFail("Primary expected"));

    fex.Seq(s => s.RefName("primary")
        .OneOf(o => o
            .Seq(e => e.Ch('(').Ref("expr").Ch(')').OnFail(") expected"))
            .Seq(s => s.NumDecimal(n => numStack.Push(n)))
         ));

    // Axiom (= where we start)
    var exprEval = fex.Seq(s => s.SetPreOp(c => c.SkipSp()).Fex(expr).IsEos().OnFail("invalid expression"));

    Console.WriteLine(exprEval.Run(scn) ? $"Passed = {numStack.Pop():F4}"
                                        : parseError.AsConsoleError("Expression Error:"));
}
```

Example error reporting for the expression parser above:

```dos
Expression Error:
9 - ( 5.5 ++ 3 ) * 6 - 4 / ( 9 - 1 )
-----------^ (ln:1 Ch:12)
Parse error: Primary expected
```

## Quick Start

Available via Nuget

> **Note:** Flow Expressions are actually implemented in the ScriptUtils library which may be found at the [script-utils repo](https://github.com/PromicSW/script-utils)

Below is a basic console application that defines and runs a Flow Expression:
```csharp
using Psw.ScriptUtil;
using Psw.ScriptUtil.FlowExpressions;
using Psw.ScriptUtil.Scanners;

QuickStart();

void QuickStart() {
    /* Parse demo telephone number of the form:
     *   (dialing_code) area_code-number: E.g (011) 734-9571
     *     - dialing code: 3 or more digits enclosed in (..)
     *     - Followed by optional spaces
     *     - area_code: 3 digits
     *     - Single space or -
     *     - number: 4 digits
     */

    var fex = new FlowExpression<FexScanner>();  // Flow Expression using FexScanner
    string dcode = "", acode = "", number = "";  // Will record the values in here

    // Build the flow expression with 'Axiom' tnumber:
    var tnumber = fex.Seq(s => s
        .Ch('(').OnFail("( expected")
        .Rep(3, -1, r => r.Digit(v => dcode += v)).OnFail("at least 3 digit dialing code expected")
        .Ch(')').OnFail(") expected")
        .Sp()
        .Rep(3, r => r.Digit(v => acode += v)).OnFail("3 digit area code expected")
        .AnyCh("- ").OnFail("- or space expected")
        .Rep(4, r => r.Digit(v => number += v)).OnFail("4 digit number expected")
    );

    var testNumber = "(011) 734-9571";
    var scn = new FexScanner(testNumber);  // Scanner instance

    // Run the Axiom and output results:
    if (tnumber.Run(scn)) 
        Console.WriteLine($"TNumber OK: ({dcode}) {acode}-{number}");      // Passed: Display result
    else 
        Console.WriteLine(scn.ErrorLog.AsConsoleError("TNumber Error:")); // Failed: Display formatted error

}
```

## Reference Documents
- [Fex Element reference](Docs/FexElementsRef.md): Complete reference of all the Fex Elements (building blocks).
- [FexScanner](Docs/FexScannerExt.md): The supplied Fex scanner and context extensions reference.
- [Custom Scanner tutorial](Docs/CustomScanner.md): Describes how to build a custom scanner with examples.
- **Note:** Flow Expressions are actually implemented in the ScriptUtils library which may be found at the [script-utils repo](https://github.com/PromicSW/script-utils)

## About

Flow Expressions provide a novel mechanism of constructing parsers using a simple fluent syntax. 
Having developed several programming languages and parsers in the past I came up with this idea as an experiment. 
It turned out so well, and I find it far more intuitive than say Parser Combinators and PEG, that I decided to share it with you.

Flow Expressions have since been used to create sophisticated parsers for various projects:
- ElementScript
- Markdown to Html parser
- GenCodeDoc - extract and generate code reference documentation (some of reference material here was generated this way)
- Several others...