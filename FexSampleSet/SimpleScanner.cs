// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using Psw.FlowExpressions;

namespace FexExampleSet
{
    public class SimpleScanner
    {
        private string _source;
        public char Delim { get; private set; }  // Last Delimiter logged
        private int _index = 0;                  // Scan pointer / index
        protected static char _Eos = '0';        // End of source character

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

        // Return true if char at index matches any of the chars and advance index
        // Also log the char matched in Delim for later access
        // Else return false and index unchanged
        public bool IsAnyCh(string chars) {
            if (!chars.Contains(PeekCh())) return false;
            Delim = PeekCh();  // Log the char in Delim
            _index++;
            return true;
        }

        // Skip spaces
        public void SkipSp() { while (IsCh(' ')) ; }

        // Build and error message string showing error position
        public string ErrorMsg(string msg)
            => $"{_source}\r\n{new string('-', _index)}^\r\n{msg}";
    }

    public static class FexSimpleScannerExt
    {

        // Operator extension bound to scanner.IsCh(...)
        public static FexBuilder<T> Ch<T>(this FexBuilder<T> exp, char ch) where T : SimpleScanner
            => exp.Op(c => c.IsCh(ch));

        // Operator extension bound to scanner.IsAnyCh(...):
        // - IsAnyCh records the char found in scanner.Delim:
        //   - So we record this value in the Op for access via ActValue<T>(Action<T> valueAction)
        //   - Or, as below, we can directly provide an Action<char> delegate to operate on the value.
        //     E.g. AnyCh("123", c => Console.WriteLine($"Number = {c}"))
        //          rather than: AnyCh("123").ActValue<char>(c => Console.WriteLine($"Number = {c}"))
        public static FexBuilder<T> AnyCh<T>(this FexBuilder<T> exp, string matchChars, Action<char> valueAction = null) where T : SimpleScanner
            => exp.Op((c, v) => v.SetValue(c.IsAnyCh(matchChars), c.Delim)).ActValue(valueAction);

        // Operator extension to skip spaces without ever failing.
        public static FexBuilder<T> Sp<T>(this FexBuilder<T> exp) where T : SimpleScanner
            => exp.Op(c => { c.SkipSp(); return true; });

        // Override OnFail to produce console output
        public static FexBuilder<T> OnFail<T>(this FexBuilder<T> exp, string errorMsg) where T : SimpleScanner
            => exp.OnFail(c => Console.WriteLine(c.ErrorMsg(errorMsg)));

        // Override Fail to produce console output
        public static FexBuilder<T> Fail<T>(this FexBuilder<T> exp, string errorMsg) where T : SimpleScanner
            => exp.Fail(c => Console.WriteLine(c.ErrorMsg(errorMsg)));
    }

    public static class SSDemo {

        public static void DemoSimpleScanner1() {
            var scn = new SimpleScanner(" N3 N1N2-abc");
            var fex = new FlowExpression<SimpleScanner>();

            // Grammar: (space* 'N' ('1' | '2' | '3'))+ '-' 'ab'? 'c'

            var validNumber = fex.Seq(s => s
                .Op(c => c.IsAnyCh("123"))
                .OnFail(c => Console.WriteLine(c.ErrorMsg("1 2 or 3 expected")))
                .Act(c => Console.WriteLine($"N value = N{c.Delim}"))
            );

            var after = fex.Seq(s => s
                .Opt(o => o.Op(c => c.IsCh('a')).Op(c => c.IsCh('b')).OnFail(c => Console.WriteLine(c.ErrorMsg("b expected"))))
                .Op(c => c.IsCh('c')).OnFail(c => Console.WriteLine(c.ErrorMsg("c expected")))
            );

            var startRep = fex.Rep1N(r => r.Op(c => c.IsCh('N')).PreOp(p => p.SkipSp()).Fex(validNumber));

            var axiom = fex.Seq(s => s.Fex(startRep).Op(c => c.IsCh('-')).Fex(after));

            if (axiom.Run(scn)) Console.WriteLine("Passed");
            else Console.WriteLine("Failed");
        }

        public static void DemoSimpleScanner2(string source = " N3 N1N2-abc") {

            // Grammar: (space* 'N' ('1' | '2' | '3'))+ '-' 'ab'? 'c'

            Console.WriteLine($"Source = \"{source}\"");

            var scn = new SimpleScanner(source);
            var fex = new FlowExpression<SimpleScanner>();

            var validNumber = fex.Seq(s => s
                .AnyCh("123", v => Console.WriteLine($"N value = N{v}"))
                .OnFail("1, 2 or 3 expected")
            );

            var after = fex.Seq(s => s
                .Opt(o => o.Ch('a').Ch('b').OnFail("b expected")) // If we have a then b must follow
                .Ch('c').OnFail("c expected")
            );

            var startRep = fex.Rep1N(r => r.Ch('N').PreOp(p => p.SkipSp()).Fex(validNumber));

            var axiom = fex.Seq(s => s.Fex(startRep).Ch('-').Fex(after));

            if (axiom.Run(scn)) Console.WriteLine("Passed");
            else Console.WriteLine("Failed");
        }
    }

}
