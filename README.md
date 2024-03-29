# Flow Expressions 


Construct, ready to run,  Parsers of any complexity using a declarative fluent syntax in C#. The system is lightweight, fast, and loosely coupled components provide complete implementation flexibility.

**Note:** This repo uses high performance scanners from the [Scanners](https://github.com/PromicSW/scanners) repo/library.

[![Nuget](https://img.shields.io/nuget/v/Psw.FlowExpressions)](https://www.nuget.org/packages/Psw.FlowExpressions/)
<!--[![Nuget](https://img.shields.io/nuget/dt/Psw.FlowExpressions)](https://www.nuget.org/packages/Psw.FlowExpression/)-->

## Building Flow Expressions

Flow Expressions are defined by a structure of *FexElements* built via a Fluent API. This defines the logical flow and operations of the expression in a very readable format. 

Any logic than can be expressed as a Flow Expression (think flow chart) may be implemented. 
For starters only **Parsers** will be discussed in this document!

> A Flow Expression operates on a user supplied **Context**, which is any environment that manages and provides input/content/state.
>
> For a Parser, the context would be a **Scanner** that manages text scanning and provides <i>Tokens</i> to operate on.<br/> 
>
> A comprehensive [FexScanner](Docs/FexScannerExt.md) is provided (derived from *ScriptScanner* in the [Scanners](https://github.com/PromicSW/scanners) repo/library) but you can roll your own if required. 

The following example is a complete **Expression Parser**, including evaluation and error reporting:

```csharp
void ExpressionEval(string calc = "9 - (5.5 + 3) * 6 - 4 / ( 9 - 1 )") 
{
    // Expression Grammar:
    //   expr    => factor ( ( '-' | '+' ) factor )* ;
    //   factor  => unary ( ( '/' | '*' ) unary )* ;
    //   unary   => ( '-'  unary ) | primary ;
    //   primary => NUMBER | '(' expression ')' ;

    // Number Stack for calculations:
    Stack<double> numStack = new Stack<double>();

    Console.WriteLine($"Calculate: {calc}");

    var fex = new FlowExpression<FexScanner>();  

    var expr = fex.Seq(s => s
        .Ref("factor")
        .RepOneOf(0, -1, r => r
            .Seq(s => s.Ch('+').Ref("factor").Act(c => numStack.Push(numStack.Pop() + numStack.Pop())))
            .Seq(s => s.Ch('-').Ref("factor").Act(c => numStack.Push(-numStack.Pop() + numStack.Pop())))
         ));

    var factor = fex.Seq(s => s.RefName("factor")
        .Ref("unary")
        .RepOneOf(0, -1, r => r
            .Seq(s => s.Ch('*').Ref("unary").Act(c => numStack.Push(numStack.Pop() * numStack.Pop())))
            .Seq(s => s.Ch('/').Ref("unary")
                       .Op(c => numStack.Peek() != 0).OnFail("Division by 0") // Trap division by 0
                       .Act(c => numStack.Push(1 / numStack.Pop() * numStack.Pop())))
         ));

    var unary = fex.Seq(s => s.RefName("unary")
        .OneOf(o => o
            .Seq(s => s.Ch('-').Ref("unary").Act(a => numStack.Push(-numStack.Pop())))
            .Ref("primary")
         ));

    var primary = fex.Seq(s => s.RefName("primary")
        .OneOf(o => o
            .Seq(e => e.Ch('(').Fex(expr).Ch(')').OnFail(") expected"))
            .Seq(s => s.NumDecimal(n => numStack.Push(n)))
         ).OnFail("Primary expected"));

    var exprEval = fex.Seq(s => s.GlobalPreOp(c => c.SkipSp()).Fex(expr).IsEos().OnFail("invalid expression"));

    var scn = new FexScanner(calc);

    Console.WriteLine(exprEval.Run(scn) 
        ? $"Answer = {numStack.Pop():F4}" 
        : scn.ErrorLog.AsConsoleError("Expression Error:"));
}
```

Example error reporting for the expression parser above:

```dos
Expression Error:
9 - ( 5.5 ++ 3 ) * 6 - 4 / ( 9 - 1 )
-----------^ (Ln:1 Ch:12)
Parse error: Primary expected
```

## Quick Start

[![Nuget](https://img.shields.io/nuget/v/Psw.FlowExpressions)](https://www.nuget.org/packages/Psw.FlowExpression/)

Below is a basic console application that defines and runs a Flow Expression to parse a fictitious telephone number:
```csharp
using Psw.FlowExpressions;

QuickStart();

void QuickStart() 
{
    /* Parse demo telephone number of the form:
     *   (dialing_code) area_code-number: E.g (011) 734-9571
     *   
     *   Grammar: '(' (digit)3+ ')' space* (digit)3 (space | '-') (digit)4
     *   - dialing code: 3 or more digits enclosed in (..)
     *   - Followed by optional spaces
     *   - area_code: 3 digits
     *   - Single space or -
     *   - number: 4 digits
     */

    var fex = new FlowExpression<FexScanner>();  // Flow Expression using FexScanner.
    string dcode = "", acode = "", number = "";  // Will record values in here.

    // Build the flow expression with 'Axiom' tnumber:
    var tnumber = fex.Seq(s => s
        .Ch('(').OnFail("( expected")
        .Rep(3, -1, r => r.Digit(v => dcode += v)).OnFail("3+ digit dialing code expected")
        .Ch(')').OnFail(") expected")
        .Sp()
        .Rep(3, r => r.Digit(v => acode += v)).OnFail("3 digit area code expected")
        .AnyCh("- ").OnFail("- or space expected")
        .Rep(4, r => r.Digit(v => number += v)).OnFail("4 digit number expected")
    );

    var testNumber = "(011) 734-9571";
    var scn = new FexScanner(testNumber);  // Scanner instance

    // Run the Axiom and output results:
    Console.WriteLine(tnumber.Run(scn)
        ? $"TNumber OK: ({dcode}) {acode}-{number}"       // Passed: Display result
        : scn.ErrorLog.AsConsoleError("TNumber Error:")); // Failed: Display formatted error

}
```

## Reference Documents
- [Overview](Docs/FexOverview.md): Introductory overview explaining how Flow Expression work.
- [Fex Element reference](Docs/FexElementsRef.md): Complete reference of all the Fex Elements (building blocks).
- [FexScanner](Docs/FexScannerExt.md): The supplied Fex scanner and context extensions reference.
- [Custom Scanner tutorial](Docs/CustomScanner.md): Describes how to build a custom scanner with examples.
- Also see the *FexSampleSet* project for all the example code and more.

## About

Flow Expressions provide a novel mechanism of constructing parsers using a simple fluent syntax. 
Having developed several programming languages and parsers in the past I came up with this idea as an experiment. 
It think it turned out very well, and find it far more functional and intuitive than say *Parser Combinators* and *PEG*, give it a try!

> It actually makes parser construction quite fun as you can achieve a lot with a few lines of code, while the system does all the heavy lifting.
>
> Maybe some of you could help with suggestions, a few nice tutorials and examples etc.

Flow Expressions have since been used to create sophisticated parsers for various projects:
- ElementScript (coming soon and it's very cool!)
- Markdown to Html parser
- CShapCodeDoc - extract and generate code reference documentation (some of the reference material here was generated this way)
- Several others...

> A potential future extension is to provide a *lookahead* mechanism - will see if there is any interest.
## Versatility of Flow Expressions
Flow Expressions are not limited to parsing and can be used to implement other kinds of *flow logic*. 

For example the FexSampleSet project contains several examples including a REPL console menu to run them.
```con
Run Sample:
  1 - Quick Start
  2 - Use Simple Scanner
  3 - Expression Eval
  4 - Expression REPL
  Blank to Exit
>
```

The REPL menu via conventional coding below:

```csharp
void RunSamples() 
{
    var samples = new List<Sample> {
        new Sample("Quick Start", () => QuickStart()),
        new Sample("Use Simple Scanner", () => DemoSimpleScanner()),
        new Sample("Expression Eval", () => ExpressionEval()),
        new Sample("Expression REPL", () => ExpressionREPL()),
    };

    while (true) {
        Console.WriteLine("Run Sample:");
        int i = 1;
        foreach (Sample sample in samples) {
            Console.WriteLine($"  {i++} - {sample.Name}");
        }
        Console.WriteLine("  Blank to Exit");
        Console.Write("> ");

        var val = Console.ReadLine();
        if (string.IsNullOrEmpty(val)) break;

        Console.Clear();

        if (int.TryParse(val, out i)) {
            if (i > 0 && i <= samples.Count) {
                Console.WriteLine($"Run: {samples[i - 1].Name}");
                samples[i - 1].Run();

                Console.WriteLine();
            }
        }
    }
}
```

An equivalent REPL menu, via a Flow Expression, shows what's possible: 

```csharp
void RunSamplesFex() 
{
    var samples = new List<Sample> {
        new Sample("Quick Start", () => QuickStart()),
        new Sample("Use Simple Scanner", () => DemoSimpleScanner()),
        new Sample("Expression Eval", () => ExpressionEval()),
        new Sample("Expression REPL", () => ExpressionREPL()),
    };

    string val = "";

    // FexNoContext is just an empty class since we don't do any scanning here.
    new FlowExpression<FexNoContext>()
        .Rep0N(r => r
            .Act(c => Console.WriteLine("Run Sample:"))
            .RepAct(samples.Count, (c, i) => Console.WriteLine($"  {i + 1} - {samples[i].Name}"))
            .Act(c => Console.Write("  Blank to Exit\r\n> "))
            .Op(o => !string.IsNullOrEmpty(val = Console.ReadLine()))
            .Act(c => {
                Console.Clear();
                if (int.TryParse(val, out int m)) {
                    if (m > 0 && m <= samples.Count) {
                        Console.WriteLine($"Run: {samples[m - 1].Name}");
                        samples[m - 1].Run();
                        Console.WriteLine();
                    }
                }
            })
        ).Run(new FexNoContext());
}
```
