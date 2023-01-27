// -----------------------------------------------------------------------------
// Copyright (c) Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using Psw.ScriptUtils.FlowExpressions;

namespace FexExampleSet
{
    public class SimpleScanner
    {
        private string _source;
        public char Delim { get; private set; }        // Last Delimiter logged
        private int _index = 0;
        protected static char _Eos = '0';              // End of source character

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
        public void SkipSp() { while (IsCh(' ')) ; }

        // Build and error message string showing error position
        public string ErrorMsg(string msg)
            => $"{_source}\r\n{new string('-', _index)}^\r\n{msg}";
    }

    public static class FexSimpleScannerExt
    {

        public static FexBuilder<T> Ch<T>(this FexBuilder<T> exp, char ch) where T : SimpleScanner
            => exp.Op(c => c.IsCh(ch));

        // Operator extension that records a value
        // Check for a char match and provide a valueAction Delegate on the value
        public static FexBuilder<T> AnyCh<T>(this FexBuilder<T> exp, string matchChars, Action<char> valueAction = null) where T : SimpleScanner
            => exp.Op((c, v) => v.SetValue(c.IsAnyCh(matchChars), c.Delim)).ActValue(valueAction);

        public static FexBuilder<T> Sp<T>(this FexBuilder<T> exp) where T : SimpleScanner
            => exp.Op(c => { c.SkipSp(); return true; });

        public static FexBuilder<T> OptSp<T>(this FexBuilder<T> exp) where T : SimpleScanner
            => exp.Opt(o => o.Op(c => {
                if (c.IsCh(' ')) {
                    //while (c.IsCh(' ')) ;
                    c.SkipSp();
                    return true;
                }
                return false;
            }));

        public static FexBuilder<T> OnFail<T>(this FexBuilder<T> exp, string errorMsg) where T : SimpleScanner
            => exp.OnFail(c => Console.WriteLine(c.ErrorMsg(errorMsg)));

        public static FexBuilder<T> Fail<T>(this FexBuilder<T> exp, string errorMsg) where T : SimpleScanner
            => exp.Fail(c => Console.WriteLine(c.ErrorMsg(errorMsg)));
    }

}
