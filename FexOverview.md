# Flow Expressions Overview

A flow expression can implement just about any type of *flow logic*. Mainly for, but not limited to, **Parser** and **DSL** (domain specific language) construction. 

Flow expressions are constructed from the various FexElements (*building blocks*) via a fluent API. These FexElements define the logical flow and operation of the expression in a very readable and maintainable format. 

> A running Flow Expression operates on a user supplied **Context**, which is any environment that manages and provides input/content/state.
>
> For a Parser, the context would be a **Scanner** that manages text scanning and provides <i>Tokens</i> to operate on.<br/> 
>
> A comprehensive [FexScanner](Docs/FexScannerExt.md) is provided (derived from *ScriptScanner* in the [Scanners](https://github.com/PromicSW/scanners) repo/library) but you can roll your own if required. 

## FexElements perform several types of functions:

> Please see the [Fex Element reference](Docs/FexElementsRef.md) section for full details.<br>
> Also [FexScanner context extensions](Docs/FexScannerExt.md) for extensions specific to the FexScanner context.

- **Operators (Op):** Perform a operation on the context, returning a success status (true/false). 
    - Certain Op's produce and record a value for later use via one of the Action elements.
    - An Op can operate on the context directly, but typically *Operator Extension methods* are defined to create re-usable (and more readable) operators specific to the context used.
    - There is also a facility to attach Pre-Operations to operators (e.g skip spaces when scanning etc.)
- **Sequence(Seq):** A sequence is the primary construct used in flow expressions and defines a series of steps (1..n) to complete: 
    - A step is any FexElement.
    - All steps is a sequence must complete else the sequence fails.
    - A step(s) may be optional and there are several rules governing this.
- **Flow Control:** These elements control the flow of an expression:
    - Opt: Optional sequence.
    - OneOf: One of a set of sequences must pass.
    - NotOneOf: Inverse of OneOf.
    - Rep...: Repeat sequences.

- **Actions:** Perform actions based on the current state of the production to check values or perform any other actions required on the context:
    - Actions don't affect the validity of a sequence.
    - There are general actions and those that operate on a value recorded by and operator.
    - An Action can operate on the context directly, but typically *Action Extension methods* are defined to create re-usable (and more readable) actions specific to the context used.
- **Error reporting:** Elements for error handling and reporting.
- **Tracing:** Tracing elements primarily for debugging purposes.
