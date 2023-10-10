# Flow Expressions 


Construct, ready to run,  Parsers of any complexity using a declarative fluent syntax in C#. The system is lightweight, fast, and loosely coupled components provide complete implementation flexibility.

## Building Flow Expressions

Flow Expressions are defined by a structure of *FexElements* built via a Fluent API. This defines the logical flow and operations of the expression in a very readable format. 

Any logic than can be expressed as a Flow Expression (think flow chart) may be implemented. 


> A Flow Expression operates on a user supplied **Context**, which is any environment that manages and provides input/content/state.
>
> For a Parser, the context would be a **Scanner** that manages text scanning and provides <i>Tokens</i> to operate on. A comprehensive FexScanner is provided as the default - but you can roll your own if required. 

<br>

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

<br>

Example **error reporting** for the expression parser above:

```dos
Expression Error:
9 - ( 5.5 ++ 3 ) * 6 - 4 / ( 9 - 1 )
-----------^ (Ln:1 Ch:12)
Parse error: Primary expected
```