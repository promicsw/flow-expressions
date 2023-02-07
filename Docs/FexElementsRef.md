

<a id="id-toc"></a>
# Fex Elements Reference

The *productions* of a Flow Expression are constructed via the various FexElements which will be discussed in the following sections. 

The expression is constructed via a FexBuilder class that provides the fluent 
mechanism via methods of the following basic form (where T is the context):
```csharp
 public FexBuilder<T> Seq(Action<FexBuilder<T>> buildFex)
```
The `FlowExpression<T>` class is used create FexElements which may be used in other productions (see Fex(...) below) or as the *Axiom* to run.
```csharp
public FexElement<T> Seq(Action<FexBuilder<T>> buildFex)
```


> **Note:** Only elements marked with an Astrix * may be constructed via the FlowExpression class.

| Fex Element | Brief |
|-------------|-------|
|[`Seq(s => s...)`*](#id-seq)| **Sequence:** Series of steps that must complete in full in order to pass.|
|[`Op(Func<Ctx, bool> op)`](#id-op)| **Operator:** Perform an operation on the Context returning a true/false result. |
|[`ValidOp(Action<Ctx> action)`](#id-validop)| **Always valid operator:** Perform and operation on the Context and always returns true. |
|[`ActValue<V>(Action<V> valueAction)`](#id-value)| **Value Action:** Bind and action to an operator that records a value.|
|[`Opt(o => o...)`* ](#id-opt)| **Optional:** Optional sequence.|
|[`OneOf(o => o...)`*](#id-oneof)| **One Of:** Set of sequences that are *Or'd* together and one of them must succeed to pass.|
|[`OptOneOf(o => o...)`*](#id-optoneof)| **Optional One Of:** Optionally perform a OneOf.|
|[`NotOneOf(o => o...)`*](#id-notoneof)| **Not One Of:** Inverse of OneOf.|
|[`BreakOn(o => o...)`](#id-notoneof)| **Alias for NotOneOf:** Reads better in loops.|
|[`Rep(repMin, repMax, r => r...)`*](#id-rep)| **Repeat:** Repeated sequences.|
|[`RepOneOf(repMin, repMax, r => r...)`*](#id-reponeof)| **Repeat One Of:** Repeated a OneOf expression.|
|[`Fex(FlowExpr1, FlowExpr2, ...)`](#id-fex)| **Include Expressions:** Include a set of previously defined sub-expressions.|
|[`Act(Action<Ctx> action)`](#id-act)| **Action:** Perform any external Action based on the current state of the production.|
|[`RepAct(int repeat, Action<Ctx, int> action)`](#id-repact)| **Repeat Action:** Perform any external repeated Action based on the current state of the production.|
|[`OnFail(Action<Ctx> failAction)`](#id-onfail)| **Fail Action:**  Perform an Action if the last operator or production failed.|
|[`Fail(Action<Ctx> failAction)`](#id-fail)| **Force Fail Action:** Force a failure and perform an Action.|
|[`Assert(Func<Ctx, bool> assert, Action<Ctx> failAction)`](#id-assert)| **Assert** if a condition is true else performs failAction.|
|[`RefName(string name),  Ref(string refName)`](#id-ref)| Forward Referencing and inclusion.|
|[`OptSelf()`](#id-optself)| Optional recursive inclusion of the current production sequence within itself.|
|[`GlobalPreOp(Action<Ctx> preOp), PreOp(Action<T> preOp)`](#id-preop)| **Pre-Operations:** Attach pre-operations to operators.|
|[`Trace(Action<Ctx, bool> traceAction)`](#id-trace)| Tracing utility.|
|[`Trace(Action<Ctx, object, bool> traceAction)`](#id-trace)| Tracing utility with value.|

---
<a id="id-seq"></a>
### Sequence: `Seq(s => s...)`
A Sequence defines a series of steps that must complete in full in order to pass. Sequences are the primary building blocks of flow expressions:

- A sequence may contain any compound (and nested) structure of elements including other sequences.
- Several elements contain *inner-sequences* as their body.
- Sequences always typically start with Operators, but may also start with Action(s).

The following is a simple example of sequences.

```csharp
// The sequence below will parse: "(ababab…)"  within brackets 3 or more times "ab"
Seq(s => s
    .Ch('(')
    .Rep(3, -1, r => r.Ch('a').Ch('b'))  // Rep(eat) has an inner sequence for "ab"
    .Ch(')')
);
```
[(toc)](#id-toc)

---
<a id="id-op"></a>
### Operator: `Op(Func<Ctx, bool> op)`

Op performs any operation on the Context (or *closure* environment) and returns a pass or failure result (true / false)

```csharp
// Op calls scanner method IsCh(...) which returns true/false
Seq(s => s.Op(c => c.IsCh('a')).Op(c => c.IsCh('b')));

// Typically Context Operator Extension (discussed later) are created to simplify
Seq(s => s.Ch('a').Ch('b'));

// An Op may have a complex implementation, if required
Op(c => {
    var fname = rootPath + c.TrimToken();
    if (!File.Exists(fname)) return c.LogError($"File not found '{fname}'", "Insert File", c.Index - 2);
    c.InsertLine(File.ReadAllText(fname));
    return true;
})
```
[(toc)](#id-toc)

### Operator: `Op(Func<Ctx, FexOpValue, bool> op)`

Use to define Op's that produce and record a value:

- Operates just like a normal Op but records a value via FexOpValue
- This form is mainly used when defining see: Context Operator Extensions
- FexOpValue is a helper utility for setting up a value:
  - Has a single method `bool SetValue(bool res, object value)` 
  - Set the value and returns true if res is true.
  - Else the value is set to null and returns false.
  - See example below.
  
```csharp
// The scanner method c.IsAnyCh(...) records the char found in c.Delim - which is used to set the value
// Else IsAnyCh(...) returns false
Op((c, v) => v.SetValue(c.IsAnyCh("+-"), c.Delim))

// This would typically be followed by a Value Action element 
.ActValue<char>(v => v...)
```
[(toc)](#id-toc)

---
<a id="id-validop"></a>
### Valid Operator: `ValidOp(Action<Ctx> action)`

ValidOp performs any operation on the Context (or *closure* environment) and always returns a pass/true.

> Useful as the last element in a OneOf set to perform a default action if required.
> Equivalent to: `Op(c => { action(c); return true; })`

```csharp
OneOf(o => o
    .Seq(...)
    .Seq(...)
     // Will execute if the above sequences fail making the OneOf valid
    .ValidOp(c => c...)
);
```

[(toc)](#id-toc)

---
<a id="id-value"></a>
### Value Action: `ActValue<V>(Action<V> valueAction)`

This binds an Action to an operator (Op) that recorded a value, and should follow directly after the Op:

- If the Op succeeds, and has a non-null value, then valueAction is performed.
- The value is recorded as an object and must be cast to the actual type before use (via V, or it may be inferred from the action).
- Note that there a several other ways to do this:
  - The Op could directly perform an operation on a value it produces.
  - Context operator extensions may include a valueAction as part of the operator.

```csharp
// Basic form where Digit() records the digit character just read
Rep(3, r => r.Digit().ActValue<char>(v => acode += v))

// Context Operator Extension configured to operate on the value directly
Rep(3, r => r.Digit(v => acode += v))
```
[(toc)](#id-toc)

---
<a id="id-opt"></a>
### Optional: `Opt(o => o...)`

Opt defines and optional sequence and the following rules apply:

- If the first step passes then the remainder must complete.
- If the first step fails the remainder is aborted.
- Note: The first step(s) may themselves be optional and in this case the following applies:
  - If any of the leading optional steps or first non-optional step passes then the remainder must complete.
  - If the leading optional step(s) and the first non-optional fail then the sequence is aborted.

```csharp
// Parses: (ab)? (c)? d
Seq(s => s
    .Opt(o => o.Ch('a').Ch('b').OnFail("b expected"))  // If a then b MUST follow
    .Opt(o => o.Ch('c'))
    .Ch('d').OnFail("d expected")
);
```
[(toc)](#id-toc)

---
<a id="id-oneof"></a>
### One Of: `OneOf(o => o...)`

OneOf defines a set of sequences that are *Or'd* together and one of them must succeed to pass:

- The sequences could be a single operator or any Fex Element
- The execution *breaks out* at the point where it succeeds - so the remainder is skipped.
- Some examples from the *expression parser* below

```csharp
Seq(s => s.RefName("unary")
    .OneOf(o => o
        .Seq(s => s.Ch('-').Ref("unary").Act(a => numStack.Push(-numStack.Pop())))
        .Ref("primary")
    ));

Seq(s => s.RefName("primary")
    .OneOf(o => o
        .Seq(e => e.Ch('(').Ref("expr").Ch(')').OnFail(") expected"))
        .Seq(s => s.NumDecimal(n => numStack.Push(n)))
    ));
```
[(toc)](#id-toc)

---
<a id="id-optoneof"></a>
### Optional One Of: `OptOneOf(o => o...)`

Optionally perform a OneOf, equivalent to: `Opt(o => o.OneOf(t => t...))`

```csharp
OptOneOf(0, -1, r => r
    .Seq(s => s.Ch('+').Ref("factor").Act(c => Calc((n1, n2) => n1 + n2)))
    .Seq(s => s.Ch('-').Ref("factor").Act(c => Calc((n1, n2) => n1 - n2)))
);
```
[(toc)](#id-toc)

---
<a id="id-notoneof"></a>
### Not One Of: `NotOneOf(o => o...) / BreakOn(o => o...)`

Inverse of OneOf where it passes if none of the inner-sequences pass. else it fails:

- Typically used at the beginning of Rep(eat) loops to break out of the loop.
- `BreakOn(o => o...)` is an alias for NotOneOf that reads better in loops.
- In the example below if any of the steps in the BreakOn sequence passes, then BreakOn/NotOneOf fails and the loop is terminated because it was the first step in the Rep inner-sequence (see Rep rules).

```csharp
Rep0N(r => {
    string attrName = null;
    r.BreakOn(b => b.IsEol().IsEos().PeekAnyCh("{}[" + lineTerm))  // Exit loop on one of these
     .OneOf(s => s.Fex(MacroExpand)
        .Seq(s => s.Ch(cfg.LineContinuationChar).SkipWS())
        // Prefix Attr:
        .Seq(a => a.AnyCh(cfg.AttrPrefixChars).Act(c => attrName = c.Delim.ToString()))
        // string literal:
        .Seq(a => a.StringDelim().StrLit().Act(c => curElm.AddAttr($"prm-{prmIndex++}", c.Token)))
    )});
```
[(toc)](#id-toc)

---
<a id="id-rep"></a>
### Repeat: `Rep(repMin, repMax, r => r...)`

Rep defines a repeated sequence and the following rules apply:

- repMin = 0: Repeat 0 to repMax times. Treats the sequence as an optional (see Opt rules)
- repMin > 0: Must repeat at least repMin times
- repMax = -1: Repeat repMin to N times.  Treats the sequence, after repMin, as an optional (see Opt rules)
- repMax > 0: Repeat repMin to repMax times and then terminates the loop.

For convenience, several Repeat configurations are available:

- `Rep(count, r => r...) -> Rep(count, count, r => r...)`
- `Rep0N(r => r...) -> Rep(0, -1, r => r...)`
- `Rep1N(r => r...) -> Rep(1, -1, r => r...)`

```csharp
Rep(3, -1, r => r.Ch('a').Ch('b'));
Rep(3, 9, r => r.Ch('a').Ch('b');)
Rep(3, r => r.Ch('a').Ch('b'));
Rep0N(r => r.Ch('a').Ch('b'));
Rep1N(r => r.Ch('a').Ch('b'));
```
[(toc)](#id-toc)

---
<a id="id-reponeof"></a>
### Repeat One Of: `RepOneOf(repMin, repMax, r => r...)`

Repeat a OneOf expression, equivalent to: `Rep(repMin, repMax, r => r.OneOf(o => o...))`

```csharp
RepOneOf(0, -1, r => r
    .Seq(s => s.Ch('+').Ref("factor").Act(c => Calc((n1, n2) => n1 + n2)))
    .Seq(s => s.Ch('-').Ref("factor").Act(c => Calc((n1, n2) => n1 - n2)))
);
```
[(toc)](#id-toc)

---
<a id="id-fex"></a>
### Include Expressions: `Fex(FlowExpr1, FlowExpr2, ...)`

Include a set of previously defined sub-expressions:

- For complex expressions it may be easier to factorize out smaller sub-expressions which are then included to form the whole.
- A common sub-expressions may also be reused in several places using Fex(...). 
- In either case it makes the overall expression easier to read and maintain.

```csharp
var abSequence = fex.Seq(s => s.Ch('(') .Rep(3, r => r.Ch('a').Ch('b')) .Ch(')'));
var cdSequence = fex.Seq(s => s.Ch('[') .Rep(3, r => r.Ch('c').Ch('d')) .Ch(']'));

var fullSequence = fex.Seq(s => s.Ch('{').Fex(abSequence, cdSequence).Ch('}'));
```
[(toc)](#id-toc)

---
<a id="id-act"></a>
### Action: `Act(Action<Ctx> action)`

Perform any Action based on the current state of the production. E.g.:

- Set (or access) variables in the context or *closure*
- Perform operations etc.
- Note: The Act element has no affect on the validity of a sequence and may be used anywhere, even at the beginning.

```csharp
// E.g. Preform a calculation with values previously recorded
Seq(s => s.Ch('+').Ref("factor").Act(c => Calc((n1, n2) => n1 + n2)))

// E.g. Negate the top stack value
Seq(s => s.Ch('-').Ref("unary").Act(a => numStack.Push(-numStack.Pop())))
```
[(toc)](#id-toc)

---
<a id="id-repact"></a>
### Repeat Action: `RepAct(int repeat, Action<Ctx, int> action)`

Perform any repeated Action based on the current state of the production. E.g.:

- Set (or access) variables in the context or *closure*
- Perform operations etc.
- Note: The RepAct element has no affect on the validity of a sequence and may be used anywhere, even at the beginning.

```csharp
// c = context, i = 0 based index 
RepAct(samples.Count, (c, i) => Console.WriteLine($"  {i + 1} - {samples[i].Name}"))
```
[(toc)](#id-toc)

---
<a id="id-onfail"></a>
### Fail Action: `OnFail(Action<Ctx> failAction)`

Perform an Action if the last production failed:

- Typically used for error reporting.
- Valid only after an Op, Rep or OneOf, else it is ignored.

```csharp
Seq(s => s
    .Ch('(').OnFail("( expected")
    .Rep(3, -1, r => r.Digit()).OnFail("at least 3 digits expected")
    .Ch(')').OnFail(") expected")
);
```
[(toc)](#id-toc)

---
<a id="id-fail"></a>
### Force Fail Action: `Fail(Action<Ctx> failAction)`

Forces a failure and performs the failAction. Can use this as the last operation in a OneOf set for error messages / other.

```csharp
pfe.OneOf(s => s
    .Str("one")
    .Str("two")
    .Str("three")
    .Fail("one, two or three expected")
);
``` 
[(toc)](#id-toc)

---
<a id="id-assert"></a>
### Assert: `Assert(Func<Ctx, bool> assert, Action<Ctx> failAction)`

Assert if a condition is true. Returns false and performs failAction on failure.

```csharp
Seq(s => s.Ch('/').Ref("unary")
    .Assert(c => numStack.Peek() != 0, e => e.LogError("Division by 0")) // Trap division by 0
    .Act(c => Calc((n1, n2) => n1 / n2)))
```
[(toc)](#id-toc)

---
<a id="id-ref"></a>
### Forward Reference: `RefName(string name),  Ref(string refName)`

These elements facilitate *Forward Referencing* and/or *Recursion* (see the Expression parser for an example):

- RefName(string name): Assigns a name to the current production sequence.
- Ref(string name): References/includes a named sequence in the current sequence.

Segment of the expression parser below

```csharp
Seq(s => s.RefName("expr") // Give this sequence a name which can be referenced later
    .Ref("factor")         // Include/reference the 'factor' sub-expression
    .RepOneOf(0, -1, r => r
        .Seq(s => s.Ch('+').Ref("factor").Act(c => Calc((n1, n2) => n1 + n2)))
        .Seq(s => s.Ch('-').Ref("factor").Act(c => Calc((n1, n2) => n1 - n2)))
    ));
```
[(toc)](#id-toc)

---
<a id="id-optself"></a>
### Optional Self Recursion: `OptSelf()`

Optional recursive inclusion of the current production sequence within itself.

```csharp
// Parse a series of digits via the following Grammar:
// digits => DIGIT digits | Eos
Seq(s => s.Digit().OptSelf().IsEos());
```
[(toc)](#id-toc)

---
### Recursion Mechanism notes

A Flow expression implements recursion via Forward Referencing, OptSelf or Fex inclusion.

> **Note** Flow Expressions do not support [*Left Recursion*](https://www.tutorialspoint.com/what-is-left-recursion-and-how-it-is-eliminated) (which will cause an endless loop and possibly a stack overflow)


[(toc)](#id-toc)

---
<a id="id-preop"></a>
### Pre-Operations: `GlobalPreOp(Action<Ctx> preOp), PreOp(Action<T> preOp)`

PreOps execute before an Op executes and are typically used to skip spaces, comments and newlines etc. before tokens when parsing scripts. 

A PreOp is efficient as it will execute only once while trying several *lookahead* Operations

| Name | Description |
|-------|-------------|
|`GlobalPreOp(Action<Ctx> preOp)`|Global setting to automatically attach to all operators.|
|`PreOp(Action<Ctx> preOp)`| Use directly after an operator to attach/override a PreOp|

See the Expression example which uses a SetPreOp to skip all spaces before the *tokens*

> **Note** The preOp action may be null if no PreOp should be executed.

[(toc)](#id-toc)

---
<a id="id-trace"></a>
### Tracing : `Trace(Action<Ctx, bool> traceAction)` <br/> Tracing': `Trace(Action<Ctx, object, bool> traceAction)`

Trace action to perform on last Op. Typically display a message as a debugging aid. Should directly follow last Op:

- Ctx is the Context and bool will contain the result of the Op.
- In the second Trace method, object is the value logged by the Op and must be cast to the appropriate type.

For Console output FexBuilder extensions are required, for example, as follows:

```csharp
public static class FexBuilderExt
{
    public static FexBuilder<T> CTrace<T>(this FexBuilder<T> b ,string text) 
        => b.Trace((c, res) => Console.WriteLine($"{text} : {res}"));

    public static FexBuilder<T> CTrace<T>(this FexBuilder<T> b, Func<T, string> trace) 
        => b.Trace((c, res) => Console.WriteLine($"{trace(c)} : {res}"));

    public static FexBuilder<T> CTrace<T>(this FexBuilder<T> b, Func<T, object, string> trace) 
        => b.Trace((c, v, res) => Console.WriteLine($"{trace(c, v)} : {res}"));
}
```

Example usage of the above:
```csharp
// Trace without value (i.e value is known):
Ch('+').CTrace(t => $"Ch + {t.LineRemainder()}") 

// Example output: Ch + [- 4 / ( 9 - 1 )] : False


// Trace with value:
AnyCh("+-", v => opStack.Push(v)).CTrace((c, v) => $"AnyCh val: {v}") 

// Example output: AnyCh val: + : True
```

[(toc)](#id-toc)
