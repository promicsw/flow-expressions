# FexScanner Extensions
FlowExpressions Context Operator Extensions for FexScanner (FexScanner is an alias for ScriptScanner).

These extend FexBuilder\<T> (where T is FexScanner) 
to add Op(operators) and other methods bound to FexScanner (i.e. the Context).

> **Note:** Several methods record the scanned text in Token (of the underlying scanner)
> and can be accessed via:
> - `ActToken / ActTrimToken / ActStripToken / ActTrimStripToken`.
> - `Act(c => c.Token...)`.

Basic extension example:
```csharp
public static FexBuilder<T> Ch<T>(this FexBuilder<T> exp, char ch) where T : FexScanner 
    => exp.Op(o => o.IsCh(ch)); // IsCh is a FexScanner method
```
---
### Comments:
FexScanner can skip line and block comments via `SkipWSC`:
- Default line comment starts with `//`.
- Default block comment `/* ... */`.
- These comments can be configured (or switched off) via the `FexScanner.ConfigComments(...)` method.
```csharp
// Set comment configuration:
// - For block comments Start and End must both be valid to enable block comments. 
//
// lineComment:       Line comment (null/empty for none).
// blockCommentStart: Block comment start (null/empty for none).
// blockCommentEnd:   Block comment end (null/empty for none).
// Returns: FexScanner for fluent chaining.
public FexScanner ConfigComment(string lineComment = "//", string blockCommentStart = "/*", string blockCommentEnd = "*/")
```
---
### Extensions Reference:

| Extensions | Description |
| :---- | :------ |
| ***Token Actions:*** |  |
| ``E:  ActStripToken(Action<string> actToken)`` | Perform an Action (Act) with the current Token stripped of all comments.<br/> |
| ``E:  ActToken(Action<string> actToken)`` | Perform an Action (Act) with the current Token.<br/> |
| ``E:  ActTrimStripToken(Action<string> actToken)`` | Perform an Action (Act) with the current Token trimmed and stripped of all comments.<br/> |
| ``E:  ActTrimToken(Action<string> actToken)`` | Perform an Action (Act) with the current Trimmed Token.<br/> |
| ``E:  ValidToken()`` | Check if the current Token is not null or WhiteSpace.<br/> |
| ***Core Utilities:*** |  |
| ``E:  AnyCh(string matchChars, Action<char>? valueAction = null)`` | Check if character at Index is one of the matchChars:<br/>- Optionally perform an action on the character.<br/><br/>**Returns:**<br/>True: if found, advances the Index and logs the char in Delim and Value.<br/>False: if not found and Index is unchanged. |
| ``E:  Ch(char ch)`` | Check if the character at Index matches ch and advance Index if it does.<br/> |
| ``E:  IsAnyString(IEnumerable<string> matchStrings, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`` | Check if text at Index equals any string in matchString and optionally advance Index if it matches.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Enumerable set of strings.<br/><code>advanceIndex:</code> Advance Index to just after match (default) else not.<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase).<br/><br/>**Returns:**<br/>True and matching string is logged in Match and Value, else false. |
| ``E:  IsAnyString(string matchStrings, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`` | Check if text at Index equals any string in delimited matchStrings and optionally advance the Index if it matches.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Delimited strings and first character must be the delimiter (e.g. "\|s1\|s2\|...").<br/><code>advanceIndex:</code> Advance Index to just after match (default) else not.<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True and matching string is logged in Match and Value, else false. |
| ``E:  IsDelim(char delim)`` | Check if the last Delim matches delim (for methods that log a Delim).<br/> |
| ``E:  IsEol()`` | Query if Index is at End-of-Line.<br/> |
| ``E:  IsEos()`` | Query if Index is at End-of-Source.<br/> |
| ``E:  IsMatch(string matchString, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`` | Check if the last Match matches matchString (for methods that log a Match).<br/><br/>**Parameters:**<br/><code>matchString:</code> String to match.<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase).<br/> |
| ``E:  IsString(string matchString, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`` | Check if text at Index equals matchString and optionally advance Index if it matches.<br/><br/>**Parameters:**<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/> |
| ``E:  PeekAnyCh(string matchChars, int offset = 0)`` | Check if character at relative offset to Index matches any one of the matchChars (index unchanged).<br/> |
| ``E:  PeekCh(char ch, int offset = 0)`` | Check if character at relative offset to Index matches ch (index unchanged).<br/> |
| ***Skipping Operations:*** |  |
| ``E:  Skip(char skipChar)`` | Skip while character is skipChar.<br/><br/>**Returns:**<br/>True if not Eos after skipping else false. |
| ``E:  SkipAny(string skipChars)`` | Skip while character is any of the skipChars.<br/><br/>**Returns:**<br/>True if not Eos after skipping else false. |
| ``E:  SkipTo(char termChar, bool skipOver = false)`` | Skip until the termChar is found:<br/>- Optionally skip over the delimiter if skipOver is true.<br/><br/>**Returns:**<br/>True: Found and Index at matching char or next if skipOver = true.<br/>  False: Not found or Eos. Index not changed. |
| ``E:  SkipToAny(string termChars, bool skipOver = false)`` | Skip until any one of the termChars is found, which is logged in Delim and Value:<br/>- Optionally skip over the delimiter if skipOver is true.<br/><br/>**Returns:**<br/>True: Found and Index at matching char or next if skipOver = true<br/>  False: Not found or Eos. Index not changed. |
| ``E:  SkipToStr(string str, bool skipOver = false)`` | Skip up to given str and optionally skip over it if skipOver is true.<br/><br/>**Returns:**<br/>True: Found and Index at matching start of text or just after if skipOver = true.<br/>  False: Not found or Eos. Index not changed. |
| ``E:  SkipToAnyStr(IEnumerable<string> matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`` | Skip up to first occurrence of any string in matchStrings and optionally skip over the matching string:<br/>- The matching string is logged in Match and Value.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Enumerable set of strings.<br/><code>skipOver:</code> Advance Index to just after match (default = false) else not.<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase).<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text or just after if skipOver = true.<br/>  False: Not found or Eos. Index unchanged. |
| ``E:  SkipToAnyStr(string matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`` | Skip up to first occurrence of any string in delimited matchStrings and optionally skip over the matching string:<br/>- The matching string is logged in Match and Value.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Delimited string and first character must be the delimiter (e.g. "\|s1\|s2\|...").<br/><code>skipOver:</code> Advance Index to just after match (default = false) else not.<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase).<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text or just after if skipOver = true.<br/>  False: Not found or Eos. Index unchanged. |
| ``E:  SkipToEol(bool skipOver = true)`` | Skip to Eol or Eos (last line):<br/>- Optionally skip over the Eol if skipOver is true.<br/><br/>**Returns:**<br/>False if started at Eos else True. |
| ``E:  SkipEol()`` | Skip one NewLine - must currently be at the newline (else the operation is ignored).<br/><br/>**Returns:**<br/>True if not Eos after skipping else false. |
| ``E:  SkipConsecEol()`` | Skip All consecutive NewLines - must currently be at a newline (else the operation is ignored).<br/><br/>**Returns:**<br/>True if not Eos after skipping else false. |
| ``E:  SkipWhile(Func<char, bool> predicate)`` | Skip all characters while a predicate matches.<br/> |
| ``E:  SkipBlock(string blockStart, string blockEnd, bool isOpen = false)`` | Skip a block delimited by blockStart and blockEnd: <br/>- Handles Nesting.<br/><br/>**Parameters:**<br/><code>isOpen:</code> False - current Index at start of block else Index just inside block.<br/><br/>**Returns:**<br/>True if not at the start of a non-open block or for a valid block (Index positioned after block). <br/>Else false and Logs an error (Index unchanged). |
| ***Scanning Operations:*** |  |
| ``E:  ScanTo(char delim, bool orToEos = false, bool skipOver = false)`` | Scans up to the delim or to Eos (if orToEos it true):<br/>- Optionally skip over the delimiter if skipOver is true.<br/>- Token contains the intermediate text (excluding delimiter).<br/><br/>**Returns:**<br/>True: Delimiter found or orToEos is true. Index at Eos, delimiter or after delimiter if skipOver<br/>False: Started at Eos or delimiter not found (and orToEos is false). Index unchanged. |
| ``E:  ScanToAny(string delims, bool orToEos = false)`` | Scans up to any character in delims or to Eos (if orToEos it true):<br/>- Token contains the intermediate text (excluding delimiter).<br/>- The terminating delimiter is logged in Delim and Value.<br/><br/>**Returns:**<br/>True: Delimiter found or orToEos is true. Index at delimiter or Eos.<br/>False: Started at Eos, delimiter not found (and orToEos is false) or delims is blank. Index unchanged. |
| ``E:  ScanToStr(string findString, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`` | Scan up to a match of findString: <br/>- Token contains the intermediate text (excluding findString).<br/><br/>**Parameters:**<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase).<br/><br/>**Returns:**<br/>True:  findString found and Index directly after findString<br/>  False: findString not found and Index remains at original position. |
| ``E:  ScanToAnyStr(IEnumerable<string> matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`` | Scan up to first occurrence of any string in matchStrings.<br/>- Token contains the intermediate text (excluding matching string)<br/>- The matching string is logged in Match and Value.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Enumerable set of strings.<br/><code>skipOver:</code> Advance Index to just after match (default = false) else not.<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase).<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text or just after if skipOver = true.<br/>  False: Not found or Eos. Index unchanged. |
| ``E:  ScanToAnyStr(string matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`` | Scan up to first occurrence of any string in delimited matchStrings.<br/>- Token contains the intermediate text (excluding matching string).<br/>- The matching string is logged in Match and Value.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Delimited string and first character must be the delimiter (e.g. "\|s1\|s2\|...").<br/><code>skipOver:</code> Advance Index to just after match (default = false) else not.<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase).<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text or just after if skipOver = true.<br/>  False: Not found or Eos. Index unchanged. |
| ``E:  ScanToEol(bool skipEol = true)`` | Scan to Eol and optionally skip over Eol:<br/>- Handles intermediate or last line (with no Eol).<br/>- Token contains the intermediate text (excluding the newline).<br/><br/>**Returns:**<br/>False if started at Eos else true. |
| ``E:  ValueToEol(bool skipEol = true)`` | Scan a value (token) to Eol and optionally skip over Eol:<br/>- Handles intermediate or last line (with no Eol).<br/>- Token contains the intermediate text (excluding the newline).<br/><br/>**Returns:**<br/>False if started at Eos or a non-valid Token else true. |
| ``E:  ScanWhile(Func<TextScanner, char, int, bool> predicate)`` | Scan all characters while a predicate matches:<br/>- Predicate = Func&lt;current char, index from starting position, bool><br/>- Token contains the scanned characters string.<br/><br/>**Returns:**<br/>True if any characters are scanned (Index after last match) else false (Index unchanged) |
| ***Type Operations:*** |  |
| ``E:  Digit(Action<char>? valueAction = null)`` | Scan a digit character and perform valueAction on it if valid, else fails.<br/> |
| ``E:  NumDecimal(Action<double>? valueAction = null)`` | Scan a Decimal value and perform valueAction on it if valid, else fails.<br/> |
| ``E:  NumInt(Action<int>? valueAction = null)`` | Scan an Integer value and perform valueAction on it if valid, else fails.<br/> |
| ***Script Scanning:*** |  |
| ``E:  ScanBlock(string blockDelims = "{}", bool isOpen = false)`` | Scan a block delimited by blockDelims E.g "\{\}" or "()" or "[]" etc: <br/>- Handles Nesting and ignores any block delimiters inside comments or strings (delimited by StringDelim).<br/>- Token contains the block content excluding the block delimiters.<br/><br/>**Parameters:**<br/><code>blockDelims:</code> String with opening and closing delimiter (default = "\{\}).<br/><code>isOpen:</code> Current Index at start of block (false) else inside block.<br/><br/>**Returns:**<br/>True for a valid block (Index after block) else false and Logs an error (Index unchanged). |
| ``E:  ScanList(string delims = "()", char separator = ',', string block = "[]")`` | Scan a List of the form: ( item1, item 2 ... ):<br/>- Note: The next non-whitespace character must be the Opening list delimiter.<br/>- Item type 1: All text up to next closing delim or separator (logged trimmed).<br/>- Item type 2: A string literal - may NOT span a line! (logged verbatim excluding string delimiters).<br/>- Item type 3: Block delimited text (logged verbatim excluding block delimiters) - use for multi-line text. <br/>- Blank items are not recorded.<br/><br/>**Parameters:**<br/><code>delims:</code> Opening and closing list delimiter (default = "()").<br/><code>separator:</code> List item separator (default = ,).<br/><code>block:</code> Opening an closing Block delimiters (default = "[]").<br/><br/>**Returns:**<br/>True and List of strings is logged as a value, else false and error logged in ErrorLog. |
| ``E:  StdIdent(Action<string> actIdent)`` | Scan Standard Identifier of the form: (letter \| _)\* (letterordigit \| _)\*:<br/>- Then perform an action on the identifier if valid.<br/><br/>**Parameters:**<br/><code>actIdent:</code> Action to perform on the valid identifier (or null for no action).<br/><br/>**Returns:**<br/>True for valid identifier (and performs action) else false. |
| ``E:  StdIdent2(Action<string> actIdent)`` | Scan Standard Identifier of the form: (letter \| _)\* (letterordigit \| _ \| -)\*:<br/>- Then perform an action on the identifier if valid.<br/><br/>**Parameters:**<br/><code>actIdent:</code> Action to perform on the valid identifier (or null for no action).<br/><br/>**Returns:**<br/>True for valid identifier (and performs action) else false. |
| ``E:  StringDelim()`` | Check if current character is a string delimiter (as set in FexScanner)<br/> |
| ``E:  StrLit()`` | Scan a delimited String Literal:<br/>- Current Index must be at the starting delimiter ("\`' etc).<br/>- Token contains the string (excluding delimiters).<br/><br/>**Returns:**<br/>True: if there was a string literal and Index positioned after ending delimiter.<br/>False: for no string or Eos - Index unchanged. |
| ``E:  ValueOrStrLit(string termChars, bool orToEos = false)`` | Scan either a StrLit or result of ScanTo(termChars, orToEos):<br/>- If Index is at a StringDelim - returns the result of StrLit().<br/>- Else returns the result of ScanTo(termChars, orToEos).<br/>- Token contains the value.<br/><br/>**Returns:**<br/>Success of the scan. |
| ***Whitespace and Comment Skipping:*** |  |
| ``E:  OptSp(string spaceChars = " \t")`` | Optionally skip given space characters (default = " \t") - creates optional (Opt) Op.<br/> |
| ``E:  SkipWS(string wsChars = " \r\n\t", bool opt = false)`` | Skip given White Space characters (default: " \r\n\t").<br/><br/>**Parameters:**<br/><code>opt:</code> Make the Op optional or not.<br/><br/>**Returns:**<br/>True if not at Eos after skipping or opt == true else False. |
| ``E:  SkipWSC(bool termNL = false, string spaceChars = " \t", bool opt = false)`` | Skip White Space and comments:<br/>- White space: spaceChars + "\r\n" if termNL is false.<br/>- Block comments handle nesting and comments embedded in delimited strings.<br/>- Set termNL to true to stop at a newline, including the newline at the end of a line comment.<br/><br/>**Parameters:**<br/><code>termNL:</code> True to stop at a newline, including the newline at the end of a line comment.<br/><code>spaceChars:</code> Characters to regard as white-space (default: " \t").<br/><code>opt:</code> Make the Op optional or not.<br/><br/>**Returns:**<br/>True: Whitespace and comments skipped and Index directly after, or no comment error and opt == true.<br/>  False: Eos or comment error (bad comment error is Logged. Use IsScanError() to check) - Index unchanged. |
| ``E:  Sp(string spaceChars = " \t")`` | Skip given space characters (default = " \t").<br/><br/>**Returns:**<br/>True if not at Eos after skipping else False. |
| ***Error Messages:*** |  |
| ``E:  OnFail(string errorMsg, string errorSource = "Parse error")`` | For convenience, bind OnFail to ErrorLog.<br/> |
| ``E:  Fail(string errorMsg, string errorSource = "Parse error")`` | For convenience, bind Fail to ErrorLog.<br/> |
