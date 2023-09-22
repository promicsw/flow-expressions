# Flow Expressions Overview

A flow expression can implement just about any type of *flow logic*. Mainly for, but not limited to, **Parsers** and **DSL** (domain specific language) construction. 

Flow expressions are constructed from the various **FexElements** (*building blocks*) via a fluent API. These FexElements define the logical flow and operation of the expression in a very readable and maintainable format. 

> A running Flow Expression operates on a user supplied **Context**, which is any environment that manages and provides input/content/state.
>
> For a Parser, the context would be a **Scanner** that manages text scanning and provides <i>Tokens</i> to operate on.<br/> 
>
> A comprehensive [FexScanner](Docs/FexScannerExt.md) is provided (derived from *ScriptScanner* in the [Scanners](https://github.com/PromicSW/scanners) repo/library) but you can roll your own if required. 

## FexElements perform several types of functions:

> Please see the [Fex Element reference](Docs/FexElementsRef.md) section for full details.<br>
> Also [FexScanner context extensions](Docs/FexScannerExt.md) for extensions specific to the FexScanner as context.

- **Operators (Op):** Perform a operation on the context, returning a success status (true/false). 
    - An operator is implemented via a `Func<context, bool>` delegate which can either operate on the context and/or the *closure* environment.
    - For example: if the context is a scanner then the Op would typically perform one of the scanning methods/functions.
    - Certain Op's produce and record a **value** for later use via one of the Action elements.
    - An Op can operate on the context directly, but typically *Operator Extension methods* are defined to create re-usable (and more readable) operators specific to the context used.
    - There is also a facility to attach Pre-Operations to operators (e.g skip spaces when scanning etc.)
- **Sequence(Seq):** A sequence is the primary construct used in flow expressions and defines a series of steps (1..n) to complete: 
    - A step is any FexElement.
    - All steps is a sequence must complete else the sequence fails.
    - A step(s) may be optional and there are several rules governing this (see reference section).
- **Flow Control:** These elements control the flow of an expression:
    - Opt: Optional sequence.
    - OneOf: One of a set of sequences must pass.
    - NotOneOf: Inverse of OneOf.
    - Rep...: Repeat sequences.

- **Actions:** Perform actions based on the current state of the production to check values or perform any other actions required on the context:
    - Actions don't affect the validity of a sequence, so may be included anywhere.
    - There are general actions and those that operate on a value recorded by and operator.
    - The `ActValue(...)` element is actually bound to the preceding operator that produced a value and as such must be placed directly after the operator.
    - An Action can operate on the context directly, but typically *Action Extension methods* are defined to create re-usable (and more readable) actions specific to the context used.
- **Error reporting:** Elements for error handling and reporting.
- **Tracing:** Tracing elements primarily for debugging purposes.

## Reference Example

The following is a fully commented expression evaluation example showing many of the features of a flow expressions:

```csharp
using Psw.FlowExpressions;

void RefExpressionEval(string calc = "9 - (5.5 + 3) * 6 - 4 / ( 9 - 1 )") 
{
    /*
     * Expression Grammar:
     * expression => factor ( ( '-' | '+' ) factor )* ;
     * factor     => unary ( ( '/' | '*' ) unary )* ;
     * unary      => ( '-'  unary ) | primary ;
     * primary    => NUMBER | "(" expression ")" ;
    */

    // Number Stack for calculations:
    Stack<double> numStack = new Stack<double>();

    // The FlowExpression object used to create FexElements with FexScanner as the context:
    var fex = new FlowExpression<FexScanner>();

    // Define the main expression production, which returns a Sequence element:
    var expr = fex.Seq(s => s

        // Forward reference to the factor element, which is only defined later.
        // The element is then included at this point in the sequence.
        .Ref("factor")

        // Repeat one of the contained sequences zero or more times:
        .RepOneOf(0, -1, r => r

            // If we have a '+' run factor and then add the top two values on the stack: 
            .Seq(s => s.Ch('+').Ref("factor").Act(c => numStack.Push(numStack.Pop() + numStack.Pop())))

            // If we have a '-' run factor and then subtract the top two values on the stack: 
            // o We minus the first and add the second because the stack is in reverse order.
            .Seq(s => s.Ch('-').Ref("factor").Act(c => numStack.Push(-numStack.Pop() + numStack.Pop())))
         ));

    // Define the factor production:
    var factor = fex.Seq(s => s
        .RefName("factor")  // Set the forward reference name.
        .Ref("unary")       // Forward reference to unary

        // Repeat one of the contained sequences zero or more times:
        .RepOneOf(0, -1, r => r

            // If we have a '*' run unary and then multiply the top two values on the stack: 
            .Seq(s => s.Ch('*').Ref("unary").Act(c => numStack.Push(numStack.Pop() * numStack.Pop())))

            // If we have a '/' run unary, check for division by zero and then divide the top two values on the stack:
            // Note again the stack is in reverse order.
            .Seq(s => s.Ch('/').Ref("unary")
                       .Op(c => numStack.Peek() != 0).OnFail("Division by 0") // Trap division by 0 and report as error.
                       .Act(c => numStack.Push(1 / numStack.Pop() * numStack.Pop())))
         ));

    // Define the unary production:
    var unary = fex.Seq(s => s
        .RefName("unary") // Set the forward reference name.

        // Now we either negate a unary or have a primary.
        .OneOf(o => o
            .Seq(s => s.Ch('-').Ref("unary").Act(a => numStack.Push(-numStack.Pop())))
            .Ref("primary")
         ));

    // Define the primary production:
    var primary = fex.Seq(s => s
        .RefName("primary") // Set the forward reference name.

        // Now we either have a nested expression as (expr) or a numeric value:
        .OneOf(o => o

            // Handle a nested expression in brackets and report an error for a missing closing bracket:
            // o Fex(expr) references/includes the expr element previously defined.
            // o We could have used the RefName() / Ref() combination but this is more efficient.
            // o Also Fex can take any number of elements Fex(e1, e2 ... en)
            .Seq(e => e.Ch('(').Fex(expr).Ch(')').OnFail(") expected"))

            // Ultimately we have a number which is just pushed onto the stack. 
            .Seq(s => s.NumDecimal(n => numStack.Push(n)))
         ).OnFail("Primary expected"));  // Fail with an error if not one of the above.

    // Define the Axiom element that we will run later.
    var exprEval = fex.Seq(s => s

        // Attach a pre-operation to all Op's to skip spaces before:
        // o Uses the Scanner.SkipSp() method for this.
        // o Pre-operations run efficiently only when needed. 
        .GlobalPreOp(c => c.SkipSp())

        // Reference/include the previously defined expr element
        .Fex(expr)

        // Check that we ended at end-of-source else it's an error:
        .IsEos().OnFail("invalid expression"));

    // Create the FexScanner with the calc string as source:
    var scn = new FexScanner(calc);

    // Run the Axiom with the scanner which returns true/false:
    // o If valid display the answer = top value on the stack.
    // o Else display the error logged in the scanner's shared ErrorLog.
    Console.WriteLine(exprEval.Run(scn)
        ? $"Answer = {numStack.Pop():F4}"
        : scn.ErrorLog.AsConsoleError("Expression Error:"));
}
```