﻿// -----------------------------------------------------------------------------
// Copyright (c) Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using FexExampleSet;
using Psw.ScriptUtils.FlowExpressions;
using Psw.ScriptUtils.Scanners;

RunSamples();


void RunSamples() {

    var samples = new List<Sample> {
        new Sample("Quick Start", () => QuickStart()),
        new Sample("Use Simple Scanner", () => DemoSimpleScanner()),
        new Sample("Expression Eval", () => ExpressionEval()),
        //new Sample("Expression Eval 2", () => DocExamples.ExpressionEval2()),
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
                samples[i - 1].Action();

                Console.WriteLine();
            }
        }
    }

}

// QuickStart sample 
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
        Console.WriteLine($"TNumber OK: ({dcode}) {acode}-{number}");     // Passed: Display result
    else
        Console.WriteLine(scn.ErrorLog.AsConsoleError("TNumber Error:")); // Failed: Display formatted error

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

// Expression Evaluation
void ExpressionEval() {

    /*
     * Expression Grammar:
     * expression     => factor ( ( '-' | '+' ) factor )* ;
     * factor         => unary ( ( '/' | '*' ) unary )* ;
     * unary          => ( '-' ) unary | primary ;
     * primary        => NUMBER | "(" expression ")" ;
    */

    // Stack and stack operator to evaluate the expression
    Stack<double> numStack = new Stack<double>();

    void Calc(Func<double, double, double> op) {
        double num2 = numStack.Pop(), num1 = numStack.Pop();
        numStack.Push(op(num1, num2));
    }

    var expr1 = "9 - (5.5 + 3) * 6 - 4 / ( 9 - 1 )";

    Console.WriteLine($"Evaluating expression: {expr1}");

    //double res = 9.0 - (5.5 + 3.0) * 6.0 - 4.0 / (9.0 - 1.0);
    //Console.WriteLine($"res = {res:F4}");

    var parseError = new ScanErrorLog();
    var scn = new FexScanner(expr1, parseError);
    var fex = new FlowExpression<FexScanner>();

    var expr = fex.Seq(s => s.RefName("expr")
        .Ref("factor")
        .RepOneOf(0, -1, r => r
            //.Seq(s => s.Ch('+').Ref("factor").Act(c => Calc((n1, n2) => n1 + n2)))
            .Seq(s => s.Act(c => Console.WriteLine("Try +")).Ch('+').Ref("factor").Act(c => Calc((n1, n2) => n1 + n2)))
            .Seq(s => s.Ch('-').Ref("factor").Act(c => Calc((n1, n2) => n1 - n2)))
         ));

    var factor = fex.Seq(s => s.RefName("factor")
        .Ref("unary")
        .RepOneOf(0, -1, r => r
            .Seq(s => s.Ch('*').Ref("unary").Act(c => Calc((n1, n2) => n1 * n2)))
            .Seq(s => s.Ch('/').Ref("unary")
                       .Op(c => numStack.Peek() != 0).OnFail("Division by 0") // Trap division by 0
                       .Act(c => Calc((n1, n2) => n1 / n2)))
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
         ));

    var exprEval = fex.Seq(s => s.SetPreOp(c => c.SkipSp()).Fex(expr).IsEos().OnFail("invalid expression"));

    Console.WriteLine(exprEval.Run(scn) ? $"Passed = {numStack.Pop():F4}"
                                        : parseError.AsConsoleError("Expression Error:"));
}

// Expression Evaluation REPL
// Last result is stored in variable 'a' which may be used in the next expression
void ExpressionREPL() {
    var scn = new FexScanner("");
    var fex = new FlowExpression<FexScanner>();

    double ans = 0;

    Stack<double> numStack = new Stack<double>();

    void DoOp2(Func<double, double, double> op, string msg) {
        double num2 = numStack.Pop(), num1 = numStack.Pop();
        numStack.Push(op(num1, num2));
    }

    var expr = fex.Seq(s => s.RefName("expr")
        .Ref("factor")
        .RepOneOf(0, -1, r => r
            .Seq(s => s.Ch('+').Ref("factor").Act(c => DoOp2((n1, n2) => n1 + n2, "+")))
            .Seq(s => s.Ch('-').Ref("factor").Act(c => DoOp2((n1, n2) => n1 - n2, "-")))
         ));

    var factor = fex.Seq(s => s.RefName("factor")
        .Ref("unary")
        .RepOneOf(0, -1, r => r
            .Seq(s => s.Ch('*').Ref("unary").Act(c => DoOp2((n1, n2) => n1 * n2, "*")))
            .Seq(s => s.Ch('/').Ref("unary")
                       //.Op(c => numStack.Peek() != 0).OnFail("Division by 0") // Trap division by 0
                       .Assert(c => numStack.Peek() != 0, e => e.LogError("Division by 0")) // Trap division by 0
                       .Act(c => DoOp2((n1, n2) => n1 / n2, "/")))
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
            .Seq(s => s.Ch('a').Act(c => numStack.Push(ans)))
         ));

    var exprEval = fex.Seq(s => s.SetPreOp(c => c.SkipSp()).Fex(expr).IsEos().OnFail("invalid expression"));

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

void xExpressionREPL() {
    var scn = new FexScanner("");
    var fex = new FlowExpression<FexScanner>();

    double ans = 0;

    Stack<double> numStack = new Stack<double>();

    void DoOp2(Func<double, double, double> op, string msg) {
        double num2 = numStack.Pop(), num1 = numStack.Pop();
        numStack.Push(op(num1, num2));
    }

    var expr = fex.Seq(s => s.RefName("expr")
        .SetPreOp(c => c.SkipSp())
        .Ref("factor")
        .RepOneOf(0, -1, r => r
            .Seq(s => s.Ch('+').Ref("factor").Act(c => DoOp2((n1, n2) => n1 + n2, "+")))
            .Seq(s => s.Ch('-').Ref("factor").Act(c => DoOp2((n1, n2) => n1 - n2, "-")))
         ));

    var factor = fex.Seq(s => s.RefName("factor")
        .Ref("unary")
        .RepOneOf(0, -1, r => r
            .Seq(s => s.Ch('*').Ref("unary").Act(c => DoOp2((n1, n2) => n1 * n2, "*")))
            .Seq(s => s.Ch('/').Ref("unary")
                       .Assert(c => numStack.Peek() != 0, e => e.LogError("Division by 0")) // Trap division by 0
                       .Act(c => DoOp2((n1, n2) => n1 / n2, "/")))
         ));

    var unary = fex.Seq(s => s.RefName("unary")
        .OneOf(o => o
            .Seq(s => s.Ch('-').Ref("unary").Act(a => numStack.Push(-numStack.Pop())))
            .Ref("primary")
         ));

    var primary = fex.Seq(s => s.RefName("primary")
        .OneOf(o => o
            .Seq(e => e.Ch('(').Ref("expr").Ch(')').OnFail(") expected"))
            .Seq(s => s.NumDecimal(n => numStack.Push(n)))
            .Seq(s => s.Ch('a').Act(c => numStack.Push(ans)))
         ));


    var repl = fex.Rep0N(r => r
        .Op(c => {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) return false;
            c.SetSource(line);
            //ans = 0;
            if (expr.Run(scn)) {
                ans = numStack.Pop();
                Console.WriteLine($"= {ans:F4}");
            }
            //else Console.WriteLine("= ?");
            else Console.WriteLine(scn.ErrorLog.AsConsoleError("Expression Error:"));
            return true;
        })
    );

    repl.Run(scn);
}

// Sample items record
public class Sample
{
    public string Name;
    public Action Action;

    public Sample(string name, Action action) {
        Name = name;
        Action = action;
    }
}
