

<a id="id-toc"></a>
# Fex Element Reference

Flow Expressions are defined by a structure of *FexElements* built via a Fluent API. This defines the logical flow and operations of the flow expression in a very readable format closely resembling a flow *grammar*:
> - **Context:** Flow expressions operate on a user supplied context, which is any environment that manages and provides input and state (and may include the *closure* environment where the expression is defined).
> - For a Parser, the context would be a **Scanner**  that manages text scanning and provides *Tokens* to operate on (a default scanner, see [FexScanner](Docs/FexScannerExt.md), is supplied).
> - In the documentation this context is denoted by **T** or **Ctx**.

## Factory: `FlowExpression<T>`

The `FlowExpression<T>` class operates as a *factory* to build and return FexElements which are used in other productions or as the *Axiom* (root) to be run.
> The innards of each element are built via the encapsulated `FexBuilder<T>` class.

**Basic mechanism:**
```csharp
using Psw.FlowExpressions;

// Create a FlowEpression factory using FexScanner as the context:
var fex = new FlowExpression<FexScanner>();

// Create a sequence element (element is of type FexElement<T>):
var element = fex.Seq(Action<FexBuilder<T>> buildFex);

// Run the element (axiom) with supplied context and process result.
bool result = element.Run(new FexScanner("text to process"));

// Handle pass or failure:
```

## `FexBuilder<T>`
The actual expression is constructed via the `FexBuilder<T>` class that provides the fluent 
mechanism with methods of the following basic form:
```csharp
 FexBuilder<T> Element(Action<FexBuilder<T>> buildFex)
```

> - The details of each element type are documented below.
> - **Note:** Only elements marked with an Astrix * may be constructed via the FlowExpression factory.

| Fex Element | Brief |
| :---------- | :---- |
| [`Seq(s => s...)*`](#id-seq) | **Sequence:** Series of steps that must complete in full in order to pass. |
| [`Op(Func<Ctx, bool> op)`](#id-op) | **Operator:** Perform an operation on the Context returning a boolean result. |
| [`ValidOp(Action<Ctx> action)`](#id-validop) | **Always valid operator:** Perform and operation on the Context and always returns true. |
| [`ActValue<V>(Action<V> valueAction)`](#id-value) | **Value Action:** Bind and action to an operator that records a value. |
| [`Opt(o => o...)*` ](#id-opt) | **Optional:** Optional sequence. |
| [`OneOf(o => o...)*`](#id-oneof) | **One Of:** Set of sequences that are *Or'd* together and one of them must succeed to pass. |
| [`OptOneOf(o => o...)`*](#id-optoneof) | **Optional One Of:** Optionally perform a OneOf. |
| [`NotOneOf(o => o...)*`](#id-notoneof) | **Not One Of:** Inverse of OneOf. |
| [`BreakOn(o => o...)`](#id-notoneof) | **Alias for NotOneOf:** Reads better in loops. |
| [`Rep(repMin, repMax, r => r...)*`<br/>`Rep(count, r => r...)*`<br/>`Rep0N(r => r...)*`<br/>`Rep1N(r => r...)`*](#id-rep) | **Repeat:** Repeated sequences. |
| [`RepOneOf(repMin, repMax, r => r...)`*](#id-reponeof) | **Repeat One Of:** Repeated a OneOf expression. |
| [`Fex(FexElement1, FexElement2, ...)`](#id-fex) | **Include FexElements:** Include a set of previously defined FexElements. |
| [`Act(Action<Ctx> action)`](#id-act) | **Action:** Perform any external Action based on the current state of the production. |
| [`RepAct(int repeat, Action<Ctx, int> action)`](#id-repact) | **Repeat Action:** Perform any external repeated Action based on the current state of the production. |
| [`OnFail(Action<Ctx> failAction)`](#id-onfail) | **Fail Action:**  Perform an Action if the last operator or production failed. |
| [`Fail(Action<Ctx> failAction)`](#id-fail) | **Force Fail Action:** Force a failure and perform an Action. |
| [`Assert(Func<Ctx, bool> assert, Action<Ctx> failAction)`](#id-assert) | **Assert** if a condition is true else performs failAction. |
| [`RefName(string name),  Ref(string refName)`](#id-ref) | Forward Referencing and inclusion.|
| [`OptSelf()`](#id-optself) | Optional recursive inclusion of the current production sequence within itself. |
| [`GlobalPreOp(Action<Ctx> preOp)`<br/>`PreOp(Action<T> preOp)`](#id-preop) | **Pre-Operators:** Attach pre-operations to operators. |
| [`Trace(Func<Ctx, string> traceMessage, int level = 0)`<br/>`TraceOp(Func<Ctx, string> traceMessage, int level = 0)`<br/>`TraceOp(Func<Ctx, object, string> traceMessage, int level)`](#id-trace) | Tracing utilities. |

---
<a id="id-seq"></a>
### `Sequence: Seq(s => s...)`
A Sequence defines a series of steps that must complete in full in order to pass. Sequences are the primary building structures of flow expressions:

- A sequence consists of one or more FexElements.
- All steps in a sequence must succeed for the sequence to pass.
- *Action* FexElements don't affect the validity of a sequence.
- Steps in the sequence may be optional (see Opt...) and these also don't affect the validity of a sequence.

The following is a simple example of sequences.

```csharp
// The sequence below will parse: "(ababab…)"  within brackets 3 or more times "ab"
Seq(s => s
    .Ch('(')
    .Rep(3, -1, r => r.Ch('a').Ch('b'))  // Rep(eat) has an inner sequence for "ab"
    .Ch(')')
);
```
[`TOC`](#id-toc)

---
<a id="id-op"></a>
### `Operator: Op(Func<Ctx, bool> op)`

Op performs any operation on the Context (or *closure* environment) and returns a success result.

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
[`TOC`](#id-toc)

### `Operator: Op(Func<Ctx, FexOpValue, bool> op)`

Used to define Op's that produce and record a value:

- Operates just like a normal Op but records a value via FexOpValue.
- This form is mainly used when defining Context Operator Extensions.
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
[`TOC`](#id-toc)

---
<a id="id-validop"></a>
### `Valid Operator: ValidOp(Action<Ctx> action)`

ValidOp performs any operation on the Context (or *closure* environment) and always returns success.

> Useful as the last element in a OneOf set to perform a default operation if required.<br/>
> Equivalent to: `Op(c => { action(c); return true; })`

```csharp
OneOf(o => o
    .Seq(...)
    .Seq(...)
     // Will execute if the above sequences fail making the OneOf valid
    .ValidOp(c => c...)
);
```

[`TOC`](#id-toc)

---
<a id="id-value"></a>
### `Value Action: ActValue<V>(Action<V> valueAction)`

This binds an Action to an operator (Op) that recorded a value, and should follow directly after the Op:

- If the Op succeeds, and has a non-null value, then valueAction is invoked.
- The value is recorded as an object and must be cast to the actual type before use (via V, or it may be inferred from the action).
- Note that there a several other ways to do this:
  - The Op could directly perform an operation on a value it produces.
  - Context operator extensions may include a valueAction as part of the operator.

```csharp
var digits = "";

// Basic form where Digit() records the digit character just read
Rep(3, r => r.Digit().ActValue<char>(v => digits += v))

// Context Operator Extension configured to operate on the value directly
Rep(3, r => r.Digit(v => digits += v))
```
[`TOC`](#id-toc)

---
<a id="id-opt"></a>
### `Optional: Opt(o => o...)`

Opt defines and optional sequence and the following rules apply:

- If the first step passes then the remainder must succeed.
- If the first step fails the remainder is aborted - without error.
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
[`TOC`](#id-toc)

---
<a id="id-oneof"></a>
### `One Of: OneOf(o => o...)`

OneOf defines a set of sequences, where one of the sequences must succeed:

- Execution *breaks out* at the point where it succeeds.
- If none of the sequences pass then the production fails.
 
 Some examples from the *expression parser* below:

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
[`TOC`](#id-toc)

---
<a id="id-optoneof"></a>
### `Optional One Of: OptOneOf(o => o...)`

Optionally perform a OneOf, equivalent to: `Opt(o => o.OneOf(t => t...))`  

Define an optional set of sequences where one of them may pass:
- Same as OneOf but does not fail if no sequence passes.
 - Execution *breaks out* at the point where it does succeeds.

```csharp
OptOneOf(o => o
    .Seq(s => s.Ch('+').Ref("factor").Act(c => Calc((n1, n2) => n1 + n2)))
    .Seq(s => s.Ch('-').Ref("factor").Act(c => Calc((n1, n2) => n1 - n2)))
);
```
[`TOC`](#id-toc)

---
<a id="id-notoneof"></a>
### `Not One Of: NotOneOf(o => o...) / BreakOn(o => o...)`

Define a set of Sequences, where it fails if any sequence passes (inverse of OneOf):<br/>
- Typically used at the beginning of Rep(eat) loops to break out of the loop.<br/>
- Or at the start of any Opt (optional) sequence to skip the remainder of the sequence.<br/>
- Execution *short circuits* if any sequence succeeds.<br/>
- If any of the sequences pass then the production fails.
- `BreakOn(o => o...)` is an alias for NotOneOf that reads better in loops.

 In the example below if any of the steps in the BreakOn sequence passes, then BreakOn/NotOneOf fails and the loop is terminated because it was the first step in the Rep inner-sequence (see Rep rules).

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
[`TOC`](#id-toc)

---
<a id="id-rep"></a>
### `Repeat: Rep(repMin, repMax, r => r...)`

Rep defines a repeated sequence and the following rules apply:

- repMin = 0: Repeat 0 to repMax times. Treats the sequence as an optional (see Opt rules).
- repMin > 0: Must repeat at least repMin times.
- repMax = -1: Repeat repMin to N times.  Treats the sequence, after repMin, as an optional (see Opt rules).
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
[`TOC`](#id-toc)

---
<a id="id-reponeof"></a>
### `Repeat One Of: RepOneOf(repMin, repMax, r => r...)`

Repeat a OneOf expression, equivalent to: `Rep(repMin, repMax, r => r.OneOf(o => o...))`

Repeat a OneOf construct and the following rules apply:
- repMin = 0: Repeat 0 to repMax times.Treats the OneOf as an optional(see Opt rules).<br/>
- repMin > 0: Must repeat at least repMin times.<br/>
- repMax = -1: Repeat repMin to N times.Treats the OneOf, after repMin, as an optional (see Opt rules).<br/>
- repMax > 0: Repeat repMin to repMax times and then terminates the loop.

```csharp
RepOneOf(0, -1, r => r
    .Seq(s => s.Ch('+').Ref("factor").Act(c => Calc((n1, n2) => n1 + n2)))
    .Seq(s => s.Ch('-').Ref("factor").Act(c => Calc((n1, n2) => n1 - n2)))
);
```
[`TOC`](#id-toc)

---
<a id="id-fex"></a>
### `Include FexElements: Fex(FexElement1, FexElement2, ...)`

Include a set of previously defined FexElements (sub-expressions):

- For complex expressions it may be easier to factorize out smaller sub-expressions which are then included to form the whole.
- A common sub-expressions may also be reused in several places using Fex(...). 
- In either case it makes the overall expression easier to read and maintain.

```csharp
var abSequence = fex.Seq(s => s.Ch('(') .Rep(3, r => r.Ch('a').Ch('b')) .Ch(')'));
var cdSequence = fex.Seq(s => s.Ch('[') .Rep(3, r => r.Ch('c').Ch('d')) .Ch(']'));

var fullSequence = fex.Seq(s => s.Ch('{').Fex(abSequence, cdSequence).Ch('}'));
```
[`toc`)](#id-toc)

---
<a id="id-act"></a>
### `Action: Act(Action<Ctx> action)`

Perform any Action based on the current state of the production. E.g.:

- Set (or access) variables in the context or *closure*.
- Perform operations etc.
- Note: The Act element has no affect on the validity of a sequence and may be used anywhere.

```csharp
// E.g. Preform a calculation with values previously recorded
Seq(s => s.Ch('+').Ref("factor").Act(c => Calc((n1, n2) => n1 + n2)))

// E.g. Negate the top stack value
Seq(s => s.Ch('-').Ref("unary").Act(a => numStack.Push(-numStack.Pop())))
```
[`TOC`](#id-toc)

---
<a id="id-repact"></a>
### `Repeat Action: RepAct(int repeat, Action<Ctx, int> action)`

Perform any repeated Action based on the current state of the production. E.g.:

- Set (or access) variables in the context or *closure*
- Perform operations etc.
- Note: The RepAct element has no affect on the validity of a sequence and may be used anywhere.

```csharp
// c = context, i = 0 based index 
RepAct(samples.Count, (c, i) => Console.WriteLine($"  {i + 1} - {samples[i].Name}"))
```
[`TOC`](#id-toc)

---
<a id="id-onfail"></a>
### `Fail Action: OnFail(Action<Ctx> failAction)`

Perform a Fail Action if the last Op (or derivative), Rep or OneOf failed:

- Typically used for error reporting.
- Valid only after an Op, Rep or OneOf, else it is ignored.
- Invoked only if the last Op, Rep or OneOf failed.

```csharp
Seq(s => s
    .Ch('(').OnFail("( expected")
    .Rep(3, -1, r => r.Digit()).OnFail("at least 3 digits expected")
    .Ch(')').OnFail(") expected")
    .OneOf(s => s
        .Str("one")
        .Str("two")
        .Str("three")
    ).OnFail("one, two or three expected");
);
```
[`TOC`](#id-toc)

---
<a id="id-fail"></a>
### `Force a Fail Action: Fail(Action<Ctx> failAction)`

Forces a failure and performs the failAction. Can use this as the last operation in a OneOf set for error messages / other.

```csharp
pfe.OneOf(s => s
    .Str("one")
    .Str("two")
    .Str("three")
    .Fail("one, two or three expected")
);
``` 
[`TOC`](#id-toc)

---
<a id="id-assert"></a>
### `Assert: Assert(Func<Ctx, bool> assert, Action<Ctx> failAction)`

Assert if a condition is true. Returns false and performs failAction on failure.

```csharp
Seq(s => s.Ch('/').Ref("unary")
    .Assert(c => numStack.Peek() != 0, e => e.LogError("Division by 0")) // Trap division by 0
    .Act(c => Calc((n1, n2) => n1 / n2)))
```
[`TOC`](#id-toc)

---
<a id="id-ref"></a>
### `Forward Reference: RefName(string name),  Ref(string refName)`

These elements facilitate *Forward Referencing* and/or *Recursion* (see the Expression parser for an example):

- RefName(string name): Assigns a name to the current production/FexElement.
- Ref(string refName): References/includes a named production/FexElement in the current sequence.
- **Note:** A reference name lookup is not case sensitive.
- This is similar to `Fex(FexElement, ...)`. Fex(...) should be used if the element/sub-expression is previously defined, as it is more efficient.

Segment of the expression parser below: 

```csharp
Seq(s => s.RefName("expr") // Give this sequence a name which can be referenced later
    .Ref("factor")         // Include/reference the 'factor' sub-expression
    .RepOneOf(0, -1, r => r
        .Seq(s => s.Ch('+').Ref("factor").Act(c => Calc((n1, n2) => n1 + n2)))
        .Seq(s => s.Ch('-').Ref("factor").Act(c => Calc((n1, n2) => n1 - n2)))
    ));
```
[`TOC`](#id-toc)

---
<a id="id-optself"></a>
### `Optional Self Recursion: OptSelf()`

Optional recursive inclusion of the current production sequence within itself.

```csharp
// Parse a series of digits via the following Grammar:
// digits => DIGIT digits | Eos
Seq(s => s.Digit().OptSelf().IsEos());
```
[`TOC`](#id-toc)


### Recursion Mechanism notes:

A Flow expressions implement recursion via Forward Referencing, OptSelf or Fex inclusion.

> **Note** Flow Expressions do not support [*Left Recursion*](https://en.wikipedia.org/wiki/Left_recursion) (which will cause an endless loop and possibly a stack overflow)


[`TOC`](#id-toc)

---
<a id="id-preop"></a>
### Pre-Operators:

Pre-operators execute before an Op (operator) as and Action on the context: 
- Typically used to skip spaces, comments and newlines etc. before tokens when parsing scripts. 
- Pre-operators are efficient an only execute once while trying several lookahead operations.

<br/>

> **`GlobalPreOp(Action<Ctx> preOp)`**  
> - Binds a pre-operator to all subsequent operators.  

<br/>

> **`PreOp(Action<Ctx> preOp)`**  
> - Use directly after an operator to bind a pre-operator to the preceding operator:
> - The preOp may be null if no PreOp should be executed.
> - The above mechanism could be used to *switch off* the GlobalPreOp's for selected Op's.

See the Expression example which uses a GlobalPreOp to skip all spaces before the *tokens*.

[`TOC`](#id-toc)

---

<a id="id-trace"></a>
### Tracing Utilities:  

Tracing is typically used for debugging purposes and facilitates a means to display and/or log trace messages for a running flow expression

> **Note:** Tracing must be switched on (by supplying an IFexTracer in the FlowExpression constructor) for this to have any effect.
 
**IFexTracer:**
```csharp
public interface IFexTracer
{
    // General purpose tracing message.
    void Trace(string message, int level);

    // Tracing message with the result of an operator.
    void Trace(string message, bool pass, int level);
}
```

#### Three different forms of tracing are described below:

> **`Trace(Function<Ctx, string> traceMessage, int level = 0)`**  
> - Produce a Trace message via the context and assign a level  
> - Calls `IFexTracer.Trace(message, level)`

<br/>

> **`TraceOp(Func<Ctx, string> traceMessage, int level = 0)`**    
> - Bind a trace message to the preceding operator:
> - Produce a Trace message via the context and assign a level.  
> - Calls `IFexTracer.Trace(message, pass, level)` where pass is the result of the preceding operator.

<br/>

> **`TraceOp(Func<Ctx, object, string> traceMessage)`**  
> - Bind a trace message to the preceding operator that produces a value:
> - Produce a Trace message via the context, value (as an object) and assign a level. 
> - Calls `IFexTracer.Trace(message, pass, level)` where pass is the result of the preceding operator.

<br/>

**A sample IFexTracer for console output:**

```csharp
public class ConsoleTracer : IFexTracer
{
    public void Trace(string message, int level) {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"{new String(' ', level * 2)}{message}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    public void Trace(string message, bool pass, int level) {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"{new String(' ', level * 2)}{message} [{pass}]");
        Console.ForegroundColor = ConsoleColor.White;
    }
}
```

<br/>

**Example usage of the above:**
```csharp

// Enable tracing via an IFexTracer in the FlowExpression constructor.
var fex = new FlowExpression<FexScanner>(new ConsoleTracer());

// Trace state:
var after = fex.Seq(s => s.Trace(c => "try after sequence.")
        .Opt(o => o.Ch('a').Ch('b').OnFail("b expected")) // If we have a then b must follow
        .Ch('c').OnFail("c expected")
    );

// Example output: try after sequence.

// Trace Op without value (since value is known):
Ch('+').TraceOp(c => "Check for +") 

// Example output: Check for + [False]

// Trace Op with value:
AnyCh("+-", v => opStack.Push(v)).TraceOp((c, v) => $"AnyCh val: {v}") 

// Example output: AnyCh val: + [True]
```

[`TOC`](#id-toc)
