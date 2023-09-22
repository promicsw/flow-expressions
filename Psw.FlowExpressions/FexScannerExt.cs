// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using Psw.FlowExpressions;
using Psw.Scanners;

namespace Psw.FlowExpressions
{
    /// <summary>
    /// FlowExpressions Context Operator Extensions for FexScanner (FexScanner is an alias for ScriptScanner).
    /// </summary>
    /// <mdoc>
    /// These extend FexBuilder\<T> (where T is FexScanner) 
    /// to add Op(operators) and other methods bound to FexScanner (i.e. the Context).
    /// 
    /// > **Note:** Several methods record the scanned text in Token (of the underlying scanner)
    /// > and can be accessed via:
    /// > - ActToken / ActTrimToken / ActStripToken / ActTrimStripToken.
    /// > - Act(c => c.Token...).
    /// 
    /// Basic extension example:
    /// ```csharp
    /// public static FexBuilder<T> Ch<T>(this FexBuilder<T> exp, char ch) where T : FexScanner 
    ///     => exp.Op(o => o.IsCh(ch)); // IsCh is a FexScanner method
    /// ```
    /// <br/>
    /// 
    /// **Extensions Reference:**
    /// </mdoc>
    public static class FexScannerExt
    {
        // Token Operations ===================================================

        /// <sgroup>Token operations</sgroup>
        /// <summary>
        /// Perform an Action (Act) with the current Token.
        /// </summary>
        public static FexBuilder<T> ActToken<T>(this FexBuilder<T> exp, Action<string> actToken) where T : FexScanner
            => actToken == null ? exp : exp.Act(c => actToken(c.Token));

        /// <summary>
        /// Perform an Action (Act) with the current Trimmed Token.
        /// </summary>
        public static FexBuilder<T> ActTrimToken<T>(this FexBuilder<T> exp, Action<string> actToken) where T : FexScanner
            => actToken == null ? exp : exp.Act(c => actToken(c.TrimToken));

        /// <summary>
        /// Perform an Action (Act) with the current Token stripped of all comments.
        /// </summary>
        public static FexBuilder<T> ActStripToken<T>(this FexBuilder<T> exp, Action<string> actToken) where T : FexScanner
            => actToken == null ? exp : exp.Act(c => actToken(c.StripToken));

        /// <summary>
        /// Perform an Action (Act) with the current Token trimmed and stripped of all comments.
        /// </summary>
        public static FexBuilder<T> ActTrimStripToken<T>(this FexBuilder<T> exp, Action<string> actToken) where T : FexScanner
            => actToken == null ? exp : exp.Act(c => actToken(c.TrimStripToken));

        /// <summary>
        /// Check if the current Token is not null or WhiteSpace.
        /// </summary>
        public static FexBuilder<T> ValidToken<T>(this FexBuilder<T> exp) where T : FexScanner => exp.Op(c => c.ValidToken());

        // Core Utilities =====================================================

        /// <sgroup>Core Utilities</sgroup>
        /// <summary>
        /// Query if Index is at End-of-Line.
        /// </summary>
        public static FexBuilder<T> IsEol<T>(this FexBuilder<T> exp) where T : FexScanner => exp.Op(c => c.IsEol);

        /// <summary>
        /// Query if Index is at End-of-Source.
        /// </summary>
        public static FexBuilder<T> IsEos<T>(this FexBuilder<T> exp) where T : FexScanner => exp.Op(c => c.IsEos);


        /// <summary>
        /// Check if character at relative offset to Index matches ch (index unchanged).
        /// </summary>
        public static FexBuilder<T> PeekCh<T>(this FexBuilder<T> exp, char ch, int offset = 0) where T : FexScanner => exp.Op(c => c.IsPeekCh(ch, offset));

        /// <summary>
        /// Check if character at relative offset to Index matches any one of the matchChars (index unchanged).
        /// </summary>
        public static FexBuilder<T> PeekAnyCh<T>(this FexBuilder<T> exp, string matchChars, int offset = 0) where T : FexScanner => exp.Op(c => c.IsPeekAnyCh(matchChars, offset));

        /// <summary>
        /// Check if the character at Index matches ch and advance Index if it does. 
        /// </summary>
        public static FexBuilder<T> Ch<T>(this FexBuilder<T> exp, char ch) where T : FexScanner => exp.Op(o => o.IsCh(ch));

        /// <summary>
        /// Check if character at Index is one of the matchChars:<br/>
        /// - Optionally perform an action on the character.
        /// </summary>
        /// <returns>
        /// True: if found, advances the Index and logs the char in Delim and Value.<br/>
        /// False: if not found and Index is unchanged.
        /// </returns>
        public static FexBuilder<T> AnyCh<T>(this FexBuilder<T> exp, string matchChars, Action<char>? valueAction = null) where T : FexScanner
            => exp.Op((c, v) => v.SetValue(c.IsAnyCh(matchChars), c.Delim)).ActValue(valueAction);

        /// <summary>
        /// Check if text at Index equals matchString and optionally advance Index if it matches.
        /// </summary>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase)</param>
        public static FexBuilder<T> IsString<T>(this FexBuilder<T> exp, string matchString, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) where T : FexScanner
            => exp.Op(c => c.IsString(matchString, advanceIndex, comp));


        /// <summary>
        /// Check if text at Index equals any string in matchString and optionally advance Index if it matches.
        /// </summary>
        /// <param name="matchStrings">Enumerable set of strings.</param>
        /// <param name="advanceIndex">Advance Index to just after match (default) else not.</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase).</param>
        /// <returns>True and matching string is logged in Match and Value, else false.</returns>
        public static FexBuilder<T> IsAnyString<T>(this FexBuilder<T> exp, IEnumerable<string> matchStrings, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) where T : FexScanner
            => exp.Op((c, v) => v.SetValue(c.IsAnyString(matchStrings, advanceIndex, comp), c.Match));

        /// <summary>
        /// Check if text at Index equals any string in delimited matchStrings and optionally advance the Index if it matches.
        /// </summary>
        /// <param name="matchStrings">Delimited strings and first character must be the delimiter (e.g. "|s1|s2|...").</param>
        /// <param name="advanceIndex">Advance Index to just after match (default) else not.</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase)</param>
        /// <returns>True and matching string is logged in Match and Value, else false.</returns>
        public static FexBuilder<T> IsAnyString<T>(this FexBuilder<T> exp, string matchStrings, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) where T : FexScanner
            => exp.Op((c, v) => v.SetValue(c.IsAnyString(matchStrings, advanceIndex, comp), c.Match));

        /// <summary>
        /// Check if the last Delim matches delim (for methods that log a Delim).
        /// </summary>
        public static FexBuilder<T> IsDelim<T>(this FexBuilder<T> exp, char delim) where T : FexScanner
           => exp.Op(c => c.Delim == delim);

        /// <summary>
        /// Check if the last Match matches matchString (for methods that log a Match).
        /// </summary>
        /// <param name="matchString">String to match.</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase).</param>
        /// </summary>
        public static FexBuilder<T> IsMatch<T>(this FexBuilder<T> exp, string matchString, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) where T : FexScanner
           => exp.Op(c => c.Match != null && c.Match.Equals(matchString, comp));


        // Skip Operations ====================================================

        /// <group>Skipping Operations</group>
        /// <summary>
        /// Skip while character is skipChar.
        /// </summary>
        /// <returns>
        /// True if not Eos after skipping else false.
        /// </returns>
        public static FexBuilder<T> Skip<T>(this FexBuilder<T> exp, char skipChar) where T : FexScanner
            => exp.Op(c => c.Skip(skipChar));


        /// <summary>
        /// Skip while character is any of the skipChars.
        /// </summary>
        /// <returns>
        /// True if not Eos after skipping else false.
        /// </returns>
        public static FexBuilder<T> SkipAny<T>(this FexBuilder<T> exp, string skipChars) where T : FexScanner
            => exp.Op(c => c.SkipAny(skipChars));


        /// <summary>
        /// Skip until the termChar is found:<br/>
        /// - Optionally skip over the delimiter if skipOver is true.
        /// </summary>
        /// <returns>
        ///   True: Found and Index at matching char or next if skipOver = true.<br/>
        ///   False: Not found or Eos. Index not changed.
        /// </returns>
        public static FexBuilder<T> SkipTo<T>(this FexBuilder<T> exp, char termChar, bool skipOver = false) where T : FexScanner
            => exp.Op(c => c.SkipTo(termChar, skipOver));

        /// <summary>
        /// Skip until any one of the termChars is found, which is logged in Delim and Value:<br/>
        /// - Optionally skip over the delimiter if skipOver is true.
        /// </summary>
        /// <returns>
        ///   True: Found and Index at matching char or next if skipOver = true<br/>
        ///   False: Not found or Eos. Index not changed.
        /// </returns>
        public static FexBuilder<T> SkipToAny<T>(this FexBuilder<T> exp, string termChars, bool skipOver = false) where T : FexScanner
            => exp.Op((c, v) => v.SetValue(c.SkipToAny(termChars, skipOver), c.Delim));

        /// <summary>
        /// Skip up to given str and optionally skip over it if skipOver is true.
        /// </summary>
        /// <returns>
        ///   True: Found and Index at matching start of text or just after if skipOver = true.<br/>
        ///   False: Not found or Eos. Index not changed.
        /// </returns>
        public static FexBuilder<T> SkipToStr<T>(this FexBuilder<T> exp, string str, bool skipOver = false) where T : FexScanner
            => exp.Op(c => c.SkipToStr(str, skipOver));

        /// <summary>
        /// Skip up to first occurrence of any string in matchStrings and optionally skip over the matching string:<br/>
        /// - The matching string is logged in Match and Value.
        /// </summary>
        /// <param name="matchStrings">Enumerable set of strings.</param>
        /// <param name="skipOver">Advance Index to just after match (default = false) else not.</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase).</param>
        /// <returns>
        ///   True: Found and Index at start of matching text or just after if skipOver = true.<br/>
        ///   False: Not found or Eos. Index unchanged.
        /// </returns>
        public static FexBuilder<T> SkipToAnyStr<T>(this FexBuilder<T> exp, IEnumerable<string> matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) where T : FexScanner
            => exp.Op((c, v) => v.SetValue(c.SkipToAnyStr(matchStrings, skipOver, comp), c.Match));

        /// <summary>
        /// Skip up to first occurrence of any string in delimited matchStrings and optionally skip over the matching string:<br/>
        /// - The matching string is logged in Match and Value.
        /// </summary>
        /// <param name="matchStrings">Delimited string and first character must be the delimiter (e.g. "|s1|s2|...").</param>
        /// <param name="skipOver">Advance Index to just after match (default = false) else not.</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase).</param>
        /// <returns>
        ///   True: Found and Index at start of matching text or just after if skipOver = true.<br/>
        ///   False: Not found or Eos. Index unchanged.
        /// </returns>
        public static FexBuilder<T> SkipToAnyStr<T>(this FexBuilder<T> exp, string matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) where T : FexScanner
            => exp.Op((c, v) => v.SetValue(c.SkipToAnyStr(matchStrings, skipOver, comp), c.Match));

        /// <summary>
        /// Skip to Eol or Eos (last line):<br/>
        /// - Optionally skip over the Eol if skipOver is true.
        /// </summary>
        /// <returns> False if started at Eos else True.</returns>
        public static FexBuilder<T> SkipToEol<T>(this FexBuilder<T> exp, bool skipOver = true) where T : FexScanner
            => exp.Op(c => c.SkipToEol(skipOver));


        /// <summary>
        /// Skip one NewLine - must currently be at the newline (else the operation is ignored).
        /// </summary>
        /// <returns>
        /// True if not Eos after skipping else false.
        /// </returns>
        public static FexBuilder<T> SkipEol<T>(this FexBuilder<T> exp) where T : FexScanner
            => exp.Op(c => c.SkipEol());

        /// <summary>
        /// Skip All consecutive NewLines - must currently be at a newline (else the operation is ignored).
        /// </summary>
        /// <returns>
        /// True if not Eos after skipping else false.
        /// </returns>
        public static FexBuilder<T> SkipConsecEol<T>(this FexBuilder<T> exp) where T : FexScanner
            => exp.Op(c => c.SkipConsecEol());

        /// <summary>
        /// Skip all characters while a predicate matches.
        /// </summary>
        public static FexBuilder<T> SkipWhile<T>(this FexBuilder<T> exp, Func<char, bool> predicate) where T : FexScanner
            => exp.Op(c => { c.SkipWhile(predicate); return true; });

        /// <summary>
        /// Skip a block delimited by blockStart and blockEnd:<br /> 
        /// - Handles Nesting.
        /// </summary>
        /// <param name="isOpen">False - current Index at start of block else Index just inside block.</param>
        /// <returns>
        /// True if not at the start of a non-open block or for a valid block (Index positioned after block).<br/> 
        /// Else false and Logs an error (Index unchanged).
        /// </returns>
        public static FexBuilder<T> SkipBlock<T>(this FexBuilder<T> exp, string blockStart, string blockEnd, bool isOpen = false) where T : FexScanner
            => exp.Op(c => c.SkipBlock(blockStart, blockEnd, isOpen));

        // Scanning Operations ================================================

        /// <group>Scanning Operations</group>
        /// <summary>
        /// Scans up to the delim or to Eos (if orToEos it true):<br/>
        /// - Optionally skip over the delimiter if skipOver is true.<br/>
        /// - Token contains the intermediate text (excluding delimiter).
        /// </summary>
        /// <returns>
        /// True: Delimiter found or orToEos is true. Index at Eos, delimiter or after delimiter if skipOver<br/>
        /// False: Started at Eos or delimiter not found (and orToEos is false). Index unchanged.
        /// </returns>
        public static FexBuilder<T> ScanTo<T>(this FexBuilder<T> exp, char delim, bool orToEos = false, bool skipOver = false) where T : FexScanner
            => exp.Op(c => c.ScanTo(delim, orToEos, skipOver));

        /// <summary>
        /// Scans up to any character in delims or to Eos (if orToEos it true):<br/>
        /// - Token contains the intermediate text (excluding delimiter).<br/>
        /// - The terminating delimiter is logged in Delim and Value.
        /// </summary>
        /// <returns>
        /// True: Delimiter found or orToEos is true. Index at delimiter or Eos.<br/>
        /// False: Started at Eos, delimiter not found (and orToEos is false) or delims is blank. Index unchanged.
        /// </returns>
        public static FexBuilder<T> ScanToAny<T>(this FexBuilder<T> exp, string delims, bool orToEos = false) where T : FexScanner
            => exp.Op((c, v) => v.SetValue(c.ScanToAny(delims, orToEos), c.Delim));

        /// <summary>
        /// Scan up to a match of findString:<br/> 
        /// - Token contains the intermediate text (excluding findString).
        /// </summary>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase).</param>
        /// <returns>
        ///   True:  findString found and Index directly after findString<br/>
        ///   False: findString not found and Index remains at original position.
        /// </returns>
        public static FexBuilder<T> ScanToStr<T>(this FexBuilder<T> exp, string findString, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) where T : FexScanner
            => exp.Op(c => c.ScanToStr(findString, comp));

        /// <summary>
        /// Scan up to first occurrence of any string in matchStrings.<br/>
        /// - Token contains the intermediate text (excluding matching string)
        /// - The matching string is logged in Match and Value.
        /// </summary>
        /// <param name="matchStrings">Enumerable set of strings.</param>
        /// <param name="skipOver">Advance Index to just after match (default = false) else not.</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase).</param>
        /// <returns>
        ///   True: Found and Index at start of matching text or just after if skipOver = true.<br/>
        ///   False: Not found or Eos. Index unchanged.
        /// </returns>
        public static FexBuilder<T> ScanToAnyStr<T>(this FexBuilder<T> exp, IEnumerable<string> matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) where T : FexScanner
            => exp.Op((c, v) => v.SetValue(c.ScanToAnyStr(matchStrings, skipOver, comp), c.Match));

        /// <summary>
        /// Scan up to first occurrence of any string in delimited matchStrings.<br/>
        /// - Token contains the intermediate text (excluding matching string).
        /// - The matching string is logged in Match and Value.
        /// </summary>
        /// <param name="matchStrings">Delimited string and first character must be the delimiter (e.g. "|s1|s2|...").</param>
        /// <param name="skipOver">Advance Index to just after match (default = false) else not.</param>
        /// <param name="comp">Comparison type (default = StringComparison.InvariantCultureIgnoreCase).</param>
        /// <returns>
        ///   True: Found and Index at start of matching text or just after if skipOver = true.<br/>
        ///   False: Not found or Eos. Index unchanged.
        /// </returns>
        public static FexBuilder<T> ScanToAnyStr<T>(this FexBuilder<T> exp, string matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase) where T : FexScanner
            => exp.Op((c, v) => v.SetValue(c.ScanToAnyStr(matchStrings, skipOver, comp), c.Match));

        /// <summary>
        /// Scan to Eol and optionally skip over Eol:<br/>
        /// - Handles intermediate or last line (with no Eol).<br/>
        /// - Token contains the intermediate text (excluding the newline).
        /// </summary>
        /// <returns>False if started at Eos else true.</returns>
        public static FexBuilder<T> ScanToEol<T>(this FexBuilder<T> exp, bool skipEol = true) where T : FexScanner => exp.Op(c => c.ScanToEol(skipEol));

        /// <summary>
        /// Scan a value (token) to Eol and optionally skip over Eol:<br/>
        /// - Handles intermediate or last line (with no Eol).<br/>
        /// - Token contains the intermediate text (excluding the newline).
        /// </summary>
        /// <returns>False if started at Eos or a non-valid Token else true.</returns>
        public static FexBuilder<T> ValueToEol<T>(this FexBuilder<T> exp, bool skipEol = true) where T : FexScanner => exp.Op(c => c.ValueToEol(skipEol));

        /// <summary>
        /// Scan all characters while a predicate matches:<br />
        /// - Predicate = Func&lt;current char, index from starting position, bool><br/>
        /// - Token contains the scanned characters string.
        /// </summary>
        /// <returns>True if any characters are scanned (Index after last match) else false (Index unchanged)</returns>
        public static FexBuilder<T> ScanWhile<T>(this FexBuilder<T> exp, Func<TextScanner, char, int, bool> predicate) where T : FexScanner
            => exp.Op(c => c.ScanWhile(predicate));

        /// <summary>
        /// Scan a block delimited by blockStart and blockEnd:<br /> 
        /// - Handles Nesting.<br/>
        /// - Token contains the block content excluding the block delimiters.
        /// </summary>
        /// <param name="isOpen">False - current Index at start of block else Index just inside block.</param>
        /// <returns>
        /// True if not at the start of a non-open block or for a valid block (Index positioned after block).<br/> 
        /// Else false and Logs an error (Index unchanged).
        /// </returns>

        public static FexBuilder<T> ScanBlock<T>(this FexBuilder<T> exp, string blockStart, string blockEnd, bool isOpen = false) where T : FexScanner
            => exp.Op(c => c.ScanBlock(blockStart, blockEnd, isOpen));

        // Type Operations ====================================================

        /// <sgroup>Type Operations</sgroup>
        /// <summary>
        /// Scan an Integer value and perform valueAction on it if valid, else fails.
        /// </summary>
        public static FexBuilder<T> NumInt<T>(this FexBuilder<T> exp, Action<int>? valueAction = null) where T : FexScanner
           => exp.Op((c, v) => v.SetValue(c.NumInt(out var val), val)).ActValue(valueAction);

        /// <summary>
        /// Scan a Decimal value and perform valueAction on it if valid, else fails.
        /// </summary>
        public static FexBuilder<T> NumDecimal<T>(this FexBuilder<T> exp, Action<double>? valueAction = null) where T : FexScanner
          => exp.Op((c, v) => v.SetValue(c.NumDecimal(out var val), val)).ActValue(valueAction);

        /// <summary>
        /// Scan a digit character and perform valueAction on it if valid, else fails.
        /// </summary>
        public static FexBuilder<T> Digit<T>(this FexBuilder<T> exp, Action<char>? valueAction = null) where T : FexScanner
          => exp.Op((c, v) => v.SetValue(c.GetDigit(), c.Delim)).ActValue(valueAction);

        // Script Scanning ====================================================

        /// <sgroup>Script Scanning</sgroup>
        /// <summary>
        /// Check if current character is a string delimiter (as set in FexScanner)
        /// </summary>
        public static FexBuilder<T> StringDelim<T>(this FexBuilder<T> exp) where T : FexScanner => exp.Op(c => c.IsStringDelim());

        /// <summary>
        /// Scan a delimited String Literal:<br/>
        /// - Current Index must be at the starting delimiter ("`' etc).<br/>
        /// - Token contains the string (excluding delimiters).
        /// </summary>
        /// <returns>
        /// True: if there was a string literal and Index positioned after ending delimiter.<br/>
        /// False: for no string or Eos - Index unchanged.
        /// </returns>
        public static FexBuilder<T> StrLit<T>(this FexBuilder<T> exp) where T : FexScanner => exp.Op(c => c.StrLit());

        /// <summary>
        /// Scan either a StrLit or result of ScanTo(termChars, orToEos):<br/>
        /// - If Index is at a StringDelim - returns the result of StrLit().<br/>
        /// - Else returns the result of ScanTo(termChars, orToEos).<br/>
        /// - Token contains the value.
        /// </summary>
        /// <returns>Success of the scan.</returns>
        public static FexBuilder<T> ValueOrStrLit<T>(this FexBuilder<T> exp, string termChars, bool orToEos = false) where T : FexScanner
            => exp.Op(c => c.ValueOrStrLit(termChars, orToEos));

        /// <summary>
        /// Scan Standard Identifier of the form: (letter | _)* (letterordigit | _)*:<br/>
        /// - Then perform an action on the identifier if valid.
        /// </summary>
        /// <param name="actIdent">Action to perform on the valid identifier (or null for no action).</param>
        /// <returns>True for valid identifier (and performs action) else false.</returns>
        public static FexBuilder<T> StdIdent<T>(this FexBuilder<T> exp, Action<string> actIdent) where T : FexScanner
            => exp.Op(c => c.StdIdent()).ActToken(actIdent);

        /// <summary>
        /// Scan Standard Identifier of the form: (letter | _)* (letterordigit | _ | -)*:<br/>
        /// - Then perform an action on the identifier if valid.
        /// </summary>
        /// <param name="actIdent">Action to perform on the valid identifier (or null for no action).</param>
        /// <returns>True for valid identifier (and performs action) else false.</returns>
        public static FexBuilder<T> StdIdent2<T>(this FexBuilder<T> exp, Action<string> actIdent) where T : FexScanner =>
            exp.Op(c => c.StdIdent2()).ActToken(actIdent);

        /// <summary>
        /// Scan a block delimited by blockDelims E.g "{}" or "()" or "[]" etc:<br /> 
        /// - Handles Nesting and ignores any block delimiters inside comments or strings (delimited by StringDelim).<br/>
        /// - Token contains the block content excluding the block delimiters.
        /// </summary>
        /// <param name="blockDelims">String with opening and closing delimiter (default = "{}).</param>
        /// <param name="isOpen">Current Index at start of block (false) else inside block.</param>
        /// <returns>True for a valid block (Index after block) else false and Logs an error (Index unchanged).</returns>
        public static FexBuilder<T> ScanBlock<T>(this FexBuilder<T> exp, string blockDelims = "{}", bool isOpen = false) where T : FexScanner
            => exp.Op(c => c.ScanBlock(blockDelims, isOpen));


        /// <summary>
        /// Scan a List of the form: ( item1, item 2 ... ):<br/>
        /// - Note: The next non-whitespace character must be the Opening list delimiter.<br/>
        /// - Item type 1: All text up to next closing delim or separator (logged trimmed).
        /// - Item type 2: A string literal - may NOT span a line! (logged verbatim excluding string delimiters).
        /// - Item type 3: Block delimited text (logged verbatim excluding block delimiters) - use for multi-line text. 
        /// - Blank items are not recorded.
        /// </summary>
        /// <param name="delims">Opening and closing list delimiter (default = "()").</param>
        /// <param name="separator">List item separator (default = ,).</param>
        /// <param name="block">Opening an closing Block delimiters (default = "[]").</param>
        /// <returns>True and List of strings is logged as a value, else false and error logged in ErrorLog.</returns>
        public static FexBuilder<T> ScanList<T>(this FexBuilder<T> exp, string delims = "()", char separator = ',', string block = "[]") where T : FexScanner
            => exp.Op((c, v) => v.SetValue(c.ScanList(out var lst, delims, separator, block), lst));


        // Whitespace and Comment Skipping ====================================

        /// <sgroup>Whitespace and Comment Skipping</sgroup>
        /// <summary>
        /// Skip given space characters (default = " \t"). 
        /// </summary>
        /// <returns>True if not at Eos after skipping else False.</returns>
        public static FexBuilder<T> Sp<T>(this FexBuilder<T> exp, string spaceChars = " \t") where T : FexScanner
           => exp.Op(c => { c.SkipAny(spaceChars); return true; });

        /// <summary>
        /// Optionally skip given space characters (default = " \t") - creates optional (Opt) Op.
        /// </summary>
        public static FexBuilder<T> OptSp<T>(this FexBuilder<T> exp, string spaceChars = " \t") where T : FexScanner
           => exp.Opt(e => e.Op(c => { c.SkipAny(spaceChars); return false; }));


        /// <summary>
        /// Skip given White Space characters (default: " \r\n\t"). 
        /// </summary>
        /// <param name="opt">Make the Op optional or not.</param>
        /// <returns>True if not at Eos after skipping or opt == true else False.</returns>
        public static FexBuilder<T> SkipWS<T>(this FexBuilder<T> exp, string wsChars = " \r\n\t", bool opt = false) where T : FexScanner
            => exp.Op(c => c.SkipWS(wsChars) || opt);

        /// <summary>
        /// Skip given space characters (default = " \t"), newlines (if termNL = false) and comments //... or /*..*/ (handles nested comments):<br/>
        /// - White space: spaceChars + "\r\n" if termNL is false.<br/>
        /// - Set termNL to position Index at the next newline not inside a block comment (/*..*/), else the newlines are also skipped.
        /// </summary>
        /// <param name="opt">Make the Op optional or not.</param>
        /// <returns>
        ///   True: Whitespace and comments skipped and Index directly after, or no comment error and opt == true.<br/>
        ///   False: Eos or comment error (bad comment error is Logged. Use IsScanError() to check) - Index unchanged.
        /// </returns>
        public static FexBuilder<T> SkipWSC<T>(this FexBuilder<T> exp, bool termNL = false, string spaceChars = " \t", bool opt = false) where T : FexScanner
            => exp.Op(c => c.SkipWSC(termNL, spaceChars) || opt && !c.IsError);

        // Error Message =======================================================

        /// <group>Error Messages</group>
        /// <summary>
        /// For convenience, bind OnFail to ErrorLog.
        /// </summary>
        public static FexBuilder<T> OnFail<T>(this FexBuilder<T> exp, string errorMsg, string errorSource = "Parse error") where T : FexScanner
            => exp.OnFail(c => c.LogError(errorMsg, errorSource));

        /// <summary>
        /// For convenience, bind Fail to ErrorLog.
        /// </summary>
        public static FexBuilder<T> Fail<T>(this FexBuilder<T> exp, string errorMsg, string errorSource = "Parse error") where T : FexScanner
            => exp.Fail(c => c.LogError(errorMsg, errorSource));
    }
}
