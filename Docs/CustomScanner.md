# Custom Scanner

This tutorial describes how to construct a custom Scanner for use with Flow Expressions.

> Note: A comprehensive Scanner is included with Flow Expressions - which may be all you need

The simple Scanner below takes a source string and then provides a few scanning services:

```csharp
public class SimpleScanner {
    private string _source;
    public char Delim { get; private set; } // Last Delimiter logged
    private int _index = 0;                 // Current scan position
    protected static char _Eos = '0';       // End of source character

    public SimpleScanner(string source) => _source = source;

    // Return char at index or Eos
    public char PeekCh() => _index < _source.Length ? _source[_index] : _Eos;

    // Return true if char at index matches ch and advance index
    // Else return false and index unchanged
    public bool IsCh(char ch) {
        if (PeekCh() == ch) {
            _index++;
            return true;
        }
        return false;
    }

    // Return true if char at index matches chars, log char in Delim and advance index
    // Else return false and index unchanged
    public bool IsAnyCh(string chars) {
        if (!chars.Contains(PeekCh())) return false;
        Delim = PeekCh();  // Log the char in Delim
        _index++;
        return true;
    }

    // Skip spaces
    public void SkipSp() { while (IsCh(' ')); }

    // Build and error message string showing error position
    public string ErrorMsg(string msg)
        => $"{_source}\r\n{new string('-', _index)}^\r\n{msg}";
}
```

Now the Scanner may be used as follows:

```csharp
public static void DemoSimpleScanner1() {
    var scn = new SimpleScanner(" N3 N1N2-abc");
    var fex = new FlowExpression<SimpleScanner>();

    var validNumber = fex.Seq(s => s
        .Op(c => c.IsAnyCh("123"))
        .OnFail(c => Console.WriteLine(c.ErrorMsg("1 2 or 3 expected")))
        .Act(c => Console.WriteLine($"Number = {c.Delim}"))
    );

    var after = fex.Seq(s => s
        .Opt(o => o.Op(c => c.IsCh('a')).Op(c => c.IsCh('b')).OnFail(c => Console.WriteLine(c.ErrorMsg("b expected"))))
        .Op(c => c.IsCh('c')).OnFail(c => Console.WriteLine(c.ErrorMsg("c expected")))
    );

    var startRep = fex.Rep1N(r => r.Op(c => c.IsCh('N')).PreOp(p => p.SkipSp()).Fex(validNumber));

    var test = fex.Seq(s => s.Fex(startRep).Op(c => c.IsCh('-')).Fex(after));

    if (test.Run(scn)) Console.WriteLine("Passed");
    else Console.WriteLine("Failed");
}
```

## Context Operator Extensions
One can extend FexBuilder with operators (Op) specific to the context:
- FexBuilder\<T> (where T is the context) implements the *fluid* API for building flow expressions.
- The few extension below can be defined for our SimpleScanner.
- We can also defined extensions for *OnFail* and *Fail* to simplify things.

```csharp
public static class FexSimpleScannerExt {

    public static FexBuilder<T> Ch<T>(this FexBuilder<T> exp, char ch) where T : SimpleScanner 
        => exp.Op(c => c.IsCh(ch));

    // Operator extension that records a value
    // Check for a char match and provide a valueAction Delegate on the value
    public static FexBuilder<T> AnyCh<T>(this FexBuilder<T> exp, string matchChars, Action<char> valueAction = null) where T : SimpleScanner
        => exp.Op((c, v) => v.SetValue(c.IsAnyCh(matchChars), c.Delim)).Value(valueAction);

    public static FexBuilder<T> Sp<T>(this FexBuilder<T> exp) where T : SimpleScanner 
        => exp.Op(c => { c.SkipSp(); return true; });
    
    public static FexBuilder<T> OnFail<T>(this FexBuilder<T> exp, string errorMsg) where T : SimpleScanner
        => exp.OnFail(c => Console.WriteLine(c.ErrorMsg(errorMsg)));

    public static FexBuilder<T> Fail<T>(this FexBuilder<T> exp, string errorMsg) where T : SimpleScanner
        => exp.Fail(c => Console.WriteLine(c.ErrorMsg(errorMsg)));
}
```

And now it may be used as follows (which is much easier to read and work with)

```csharp
public static void DemoSimpleScanner2() {
    var scn = new SimpleScanner(" N3 N1N2-abc");
    var fex = new FlowExpression<SimpleScanner>();

    var validNumber = fex.Seq(s => s
        .AnyCh("123", v => Console.WriteLine($"Number = {v}"))
        .OnFail("1, 2 or 3 expected")
    );

    var after = fex.Seq(s => s
        .Opt(o => o.Ch('a').Ch('b').OnFail("b expected"))
        .Ch('c').OnFail("c expected")
    );

    var startRep = fex.Rep1N(r => r.Ch('N').PreOp(p => p.SkipSp()).Fex(validNumber));

    var test = fex.Seq(s => s.Fex(startRep).Ch('-').Fex(after));

    if (test.Run(scn)) Console.WriteLine("Passed");
    else Console.WriteLine("Failed");
}
```
