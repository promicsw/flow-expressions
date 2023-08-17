// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using FexExampleSet;
using Psw.FlowExpressions;

RunSamplesFex();


void RunSamples() {

    var samples = new List<Sample> {
        new Sample("Quick Start", () => QuickStart()),
        new Sample("Simple Scanner (valid)", () => SSDemo.DemoSimpleScanner2(" N3 N1N2-abc")),
        new Sample("Simple Scanner (invalid)", () => SSDemo.DemoSimpleScanner2(" N3 N1N2-ac")),
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

// Create a run the samples menu with a Flow Expression :)
void RunSamplesFex() {

    var samples = new List<Sample> {
        new Sample("Quick Start", () => QuickStart()),
        new Sample("Simple Scanner 1", () => SSDemo.DemoSimpleScanner1()),
        new Sample("Simple Scanner 2 (valid)", () => SSDemo.DemoSimpleScanner2(" N3 N1N3-abc")),
        new Sample("Simple Scanner 2 (invalid)", () => SSDemo.DemoSimpleScanner2(" N3 N1N2-ac")),
        new Sample("Expression Eval", () => ExpressionEval()),
        new Sample("Expression REPL", () => ExpressionREPL()),
    };

    string val = "";

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

    //var fex = new FlowExpression<FexNoContext>();

    //fex.Rep0N(r => r
    //    .Act(c => Console.WriteLine("Run Sample:"))
    //    .RepAct(samples.Count, (c, i) => Console.WriteLine($"  {i + 1} - {samples[i].Name}"))
    //    .Act(c => Console.Write("  Blank to Exit\r\n> "))
    //    .Op(o => !string.IsNullOrEmpty(val = Console.ReadLine()))
    //    .Act(c => {
    //        Console.Clear();
    //        if (int.TryParse(val, out int m)) {
    //            if (m > 0 && m <= samples.Count) {
    //                Console.WriteLine($"Run: {samples[m - 1].Name}");
    //                samples[m - 1].Run();
    //                Console.WriteLine();
    //            }
    //        }
    //    })
    //).Run(new FexNoContext());

}

// QuickStart sample 
void QuickStart() {
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
    Console.WriteLine(tnumber.Run(scn)
        ? $"TNumber OK: ({dcode}) {acode}-{number}"       // Passed: Display result
        : scn.ErrorLog.AsConsoleError("TNumber Error:")); // Failed: Display formatted error

}

// Using Simple Scanner
void DemoSimpleScanner() {
    var scn = new SimpleScanner(" N3 N1N2-abc");
    var fex = new FlowExpression<SimpleScanner>();

    var validNumber = fex.Seq(s => s
        .AnyCh("123", v => Console.WriteLine($"Number = {v}"))
        .OnFail("1, 2 or 3 expected")
    );

    var after = fex.Seq(s => s
        .Opt(o => o.Ch('a').Ch('b').OnFail("b expected")) // If we have a then b must follow
        .Ch('c').OnFail("c expected")
    );

    var startRep = fex.Rep1N(r => r.Ch('N').PreOp(p => p.SkipSp()).Fex(validNumber));

    var test = fex.Seq(s => s.Fex(startRep).Ch('-').Fex(after));

    if (test.Run(scn)) Console.WriteLine("Passed");
    else Console.WriteLine("Failed");
}

// Expression Evaluation - using FexParser
void ExpressionEval() {

    /*
     * Expression Grammar:
     * expression     => factor ( ( '-' | '+' ) factor )* ;
     * factor         => unary ( ( '/' | '*' ) unary )* ;
     * unary          => ( '-' ) unary | primary ;
     * primary        => NUMBER | "(" expression ")" ;
    */

    // Number Stack for calculations
    Stack<double> numStack = new Stack<double>();

    var expr1 = "9 - (5.5 + 3) * 6 - 4 / ( 9 - 1 )";

    Console.WriteLine($"Evaluating expression: {expr1}");

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
         ).OnFail("Primary expected"));

    var primary = fex.Seq(s => s.RefName("primary")
        .OneOf(o => o
            .Seq(e => e.Ch('(').Fex(expr).Ch(')').OnFail(") expected"))
            .Seq(s => s.NumDecimal(n => numStack.Push(n)))
         ));

    var exprEval = fex.Seq(s => s.GlobalPreOp(c => c.SkipSp()).Fex(expr).IsEos().OnFail("invalid expression"));

    var scn = new FexScanner(expr1);

    Console.WriteLine(exprEval.Run(scn) 
        ? $"Passed = {numStack.Pop():F4}" 
        : scn.ErrorLog.AsConsoleError("Expression Error:"));
}

// Expression Evaluation REPL
// Last result is stored in variable 'a' which may be used in the next expression
void ExpressionREPL() {
    var scn = new FexScanner("");
    var fex = new FlowExpression<FexScanner>();

    double ans = 0;

    Stack<double> numStack = new Stack<double>();

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
         ).OnFail("Primary expected"));

    var primary = fex.Seq(s => s.RefName("primary")
        .OneOf(o => o
            .Seq(e => e.Ch('(').Ref("expr").Ch(')').OnFail(") expected"))
            .Seq(s => s.NumDecimal(n => numStack.Push(n)))
            .Seq(s => s.Ch('a').Act(c => numStack.Push(ans)))  // a is previous answer
         ));

    var exprEval = fex.Seq(s => s.GlobalPreOp(c => c.SkipSp()).Fex(expr).IsEos().OnFail("invalid expression"));

    var repl = fex.Rep0N(r => r
        .Op(c => {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) return false;
            c.SetSource(line);

            if (exprEval.Run(scn)) {
                ans = numStack.Pop();
                Console.WriteLine($"= {ans:F4}");
            }
            else Console.WriteLine(scn.ErrorLog.AsConsoleError("Expression Error:"));
            return true;
        })
    );

    repl.Run(scn);
}

// Sample items record for menu
public class Sample
{
    public string Name;
    public Action Run;

    public Sample(string name, Action action) {
        Name = name;
        Run = action;
    }
}

