# class FexScannerExt

FlowExpressions Context Operator Extensions for FexScanner 
(FexScanner is an alias for ScriptScanner - reference in the [Script-Utils](https://github.com/PromicSW/script-utils) repo)

These extend FexBuilder\<T> (where T is FexScanner) 
to add Op(operators) and other methods bound to FexScanner (i.e. the Context)

> **Note:** Several methods record the scanned text in Token (of the underlying scanner)
> and can be accessed via:
> - ActToken / ActTrimToken
> - Act(c => c.Token...)

Basic extension example:
```csharp
public static FexBuilder<T> Ch<T>(this FexBuilder<T> exp, char ch) where T : FexScanner 
    => exp.Op(o => o.IsCh(ch)); // IsCh is a FexScanner method
```
<br/>
    

**Extensions Reference:**


|Member|Description|
|----|------|
|**Token operations:**||
|`ActToken(Action<string> actToken)`|Perform an Action (Act) with the current Token<br/>|
|`ActTrimToken(Action<string> actToken)`|Perform an Action (Act) with the current Trimmed Token<br/>|
|`ValidToken()`|Check if the current Token is not null or WhiteSpace<br/>|
|**Core Utilities:**||
|`AnyCh(string matchChars, Action<char>? valueAction = null)`|Check if character at Index is in matchChars and advance Index if it does:<br/>- Optionally perform an action on the character, which is also logged as a Value<br/>|
|`Ch(char ch)`|Check if the character at Index matches ch and advance Index if it does<br/>|
|`IsAnyString(IEnumerable<string> matchStrings, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Check if text at Index equals any string in matchString and optionally advance Index if it matches.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Enumerable set of strings<br/><code>advanceIndex:</code> Advance Index to just after match (default) else not<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True and matching string is logged as a Value, else false<br/>|
|`IsAnyString(string matchStrings, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Check if text at Index equals any string in matchString and optionally advance Index if it matches.<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Delimited string and first character must be the delimiter<br/><code>advanceIndex:</code> Advance Index to just after match (default) else not<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True and matching string is logged as a Value, else false<br/>|
|`IsEol()`|Query if Index is at End-of-Line<br/>|
|`IsEos()`|Query if Index is at End-of-Source<br/>|
|`IsString(string matchString, bool advanceIndex = true, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Check if text at Index equals matchString and optionally advance Index if it matches.<br/><br/>**Parameters:**<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/>|
|`PeekAnyCh(string matchChars)`|Check if character at Index matched any characters in matchChars (without advancing Index)<br/>|
|`PeekCh(char ch)`|Check if the character at Index matches ch (without advancing Index)<br/>|
|**Skipping Operations:**||
|`Skip(char skipChar)`|Skip while character is skipChar<br/><br/>**Returns:**<br/>True if not Eos after skipping else false<br/>|
|`SkipAny(string skipChars)`|Skip while character is any of the skipChars<br/><br/>**Returns:**<br/>True if not Eos after skipping else false<br/>|
|`SkipTo(char termChar, bool skipOver = false)`|Skip until the termChar is found:<br/>- Optionally skip over the delimiter if skipOver is true.<br/><br/>**Returns:**<br/>True: Found and Index at matching char or next if skipOver = true<br/>  False: Not found or Eos. Index not changed<br/>|
|`SkipToAny(string termChars, bool skipOver = false)`|Skip until any one of the termChars is found, which is logged as a Value<br/>- Optionally skip over the delimiter if skipOver is true.<br/><br/>**Returns:**<br/>True: Found and Index at matching char or next if skipOver = true<br/>  False: Not found or Eos. Index not changed<br/>|
|`SkipToStr(string str, bool skipOver = false)`|Skip up to given str and optionally skip over it if skipOver is true<br/><br/>**Returns:**<br/>True: Found and Index at matching start of text or just after if skipOver = true<br/>  False: Not found or Eos. Index not changed<br/>|
|`SkipToAnyStr(IEnumerable<string> matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Skip up to first occurrence of any string in matchStrings and optionally skip over the matching string.<br/>- The matching string is logged as a Value<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Enumerable set of strings<br/><code>skipOver:</code> Advance Index to just after match (default = false) else not<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text (logged as a Value) or just after if skipOver = true<br/>  False: Not found or Eos. Index unchanged<br/>|
|`SkipToAnyStr(string matchStrings, bool skipOver = false, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Skip up to first occurrence of any string in delimited matchStrings and optionally skip over the matching string.<br/>- The matching string is logged as a Value<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Delimited string and first character must be the delimiter (e.g. "\|s1\|s2\|...")<br/><code>skipOver:</code> Advance Index to just after match (default = false) else not<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text (logged as a Value) or just after if skipOver = true<br/>  False: Not found or Eos. Index unchanged<br/>|
|`SkipToEol(bool skipOver = true)`|Skip to Eol or Eos (last line)<br/>- Optionally skip over the Eol if skipOver is true.<br/><br/>**Returns:**<br/>False if started at Eos else True<br/>|
|`SkipEol()`|Skip one NewLine - must currently be at the newline (else the operation is ignored)<br/><br/>**Returns:**<br/>True if not Eos after skipping else false<br/>|
|`SkipConsecEol()`|Skip All consecutive NewLines - must currently be at a newline (else the operation is ignored)<br/><br/>**Returns:**<br/>True if not Eos after skipping else false<br/>|
|`SkipWhile(Func<char, bool> predicate)`|Skip all characters while a predicate matches:<br/><br/>**Returns:**<br/>True<br/>|
|**Scanning Operations:**||
|`ScanTo(char delim, bool orToEos = false, bool skipOver = false)`|Scans up to the delim:<br/>- Optionally skip over the delimiter if skipOver is true.<br/>- Token contains the intermediate text (excluding delimiter)<br/><br/>**Returns:**<br/>True: Delimiter found or orToEos is true. Scan pointer at Eos, delimiter or after delimiter if skipOver<br/>False: Started at Eos or delimiter not found (and orToEos is false). Scan pointer unchanged<br/>|
|`ScanToAny(string delims, bool orToEos = false)`|Scans up to any character in delims:<br/>- Token contains the intermediate text (excluding delimiter)<br/><br/>**Returns:**<br/>True: Delimiter found or orToEos is true. Scan pointer at delimiter or Eos<br/>False: Started at Eos, delimiter not found (and orToEos is false) or delims is blank. Scan pointer unchanged<br/>|
|`ScanToStr(string findString, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Scan up to a match of findString: <br/>- Token contains the intermediate text (excluding findString)<br/><br/>**Parameters:**<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True:  findString found and Index directly after findString<br/>  False: findString not found and Index remains at original position<br/>|
|`ScanToAnyStr(IEnumerable<string> matchStrings, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Scan up to first occurrence of any string in matchStrings.<br/>- Token contains the intermediate text (excluding matching string)<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Enumerable set of strings<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text<br/>  False: Not found or Eos. Index unchanged<br/>|
|`ScanToAnyStr(string matchStrings, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)`|Scan up to first occurrence of any string in delimited matchStrings.<br/>- Token contains the intermediate text (excluding matching string)<br/><br/>**Parameters:**<br/><code>matchStrings:</code> Delimited string and first character must be the delimiter (e.g. "\|s1\|s2\|...")<br/><code>comp:</code> Comparison type (default = StringComparison.InvariantCultureIgnoreCase)<br/><br/>**Returns:**<br/>True: Found and Index at start of matching text<br/>  False: Not found or Eos. Index unchanged<br/>|
|`ScanToEol(bool skipEol = true)`|Scan to Eol and optionally skip over Eol:<br/>- Handles intermediate or last line (with no Eol)<br/>- Token contains the intermediate text (excluding the newline)<br/><br/>**Returns:**<br/>False if started at Eos else true<br/>|
|`ValueToEol(bool skipEol = true)`|Scan a value (token) to Eol and optionally skip over Eol:<br/>- Handles intermediate or last line (with no Eol)<br/>- Token contains the intermediate text (excluding the newline)<br/><br/>**Returns:**<br/>False if started at Eos or a non-valid Token else true<br/>|
|`ScanWhile(Func<TextScanner, char, int, bool> predicate)`|Scan all characters while a predicate matches:<br/>- Predicate = Func&lt;current char, index from starting position, bool><br/>- Token contains the scanned characters string<br/><br/>**Returns:**<br/>True if any characters are scanned (Index after last match) else false (Index unchanged)<br/>|
|**Type Operations:**||
|`Digit(Action<char>? valueAction = null)`|Scan a digit character and perform valueAction on it if valid, else fails<br/>|
|`NumDecimal(Action<double>? valueAction = null)`|Scan a Decimal value and perform valueAction on it if valid, else fails<br/>|
|`NumInt(Action<int>? valueAction = null)`|Scan an Integer value and perform valueAction on it if valid, else fails<br/>|
|**Script Scanning:**||
|`ScanBlock(string blockDelims = "{}", bool isOpen = false)`|Scan a block delimited by blockDelims E.g "\{\}" or "()" or "[]" etc. <br/>- Handles Nesting and ignores any block delimiters inside comments or strings (delimited by StringDelim)<br/>- Token contains the block content excluding the block delimiters.<br/><br/>**Parameters:**<br/><code>blockDelims:</code> String with opening and closing delimiter (default = "\{\})<br/><code>isOpen:</code> Current Index at start of block (false) else inside block<br/><br/>**Returns:**<br/>True for a valid block (Index after block) else false and Logs an error (Index unchanged)<br/>|
|`ScanBlockStripComments(string blockDelims = "{}", bool isOpen = false)`|Scan a block delimited by blockDelims E.g "\{\}" or "()" or "[]" etc.<br/>- Strips out all comments<br/>- Handles Nesting and ignores any block delimiters inside strings (delimited by StringDelim)<br/><br/>**Parameters:**<br/><code>blockDelims:</code> String with opening and closing delimiter (default = "\{\})<br/><code>isOpen:</code> Current Index at start of block (false) else inside block<br/><br/>**Returns:**<br/>True = Success and the text logged as a Value (Index after block)<br/>False = Fail, logs the error and block = string.Empty (Index unchanged)<br/>|
|`ScanList(string delims = "()", char separator = ',', string block = "[]")`|Scan a List of the form: ( item1, item 2 ... )<br/>Item: Delimited string \| text up to next separator \| block (recorded without block delimiters)<br/>Note: Index must be at opening list delimiter<br/><br/>**Parameters:**<br/><code>delims:</code> Opening and closing list delimiter (default = "()")<br/><code>separator:</code> List item separator (default = ,)<br/><code>block:</code> Opening an closing Block delimiters (default = "[]")<br/><br/>**Returns:**<br/>True and List of strings is logged as a value, else false and error logged in ErrorLog<br/>|
|`StdIdent(Action<string> actIdent)`|Scan Standard Identifier of the form: (letter \| _)\* (letterordigit \| _)\*<br/>- Then perform an action on the identifier if valid.<br/><br/>**Parameters:**<br/><code>actIdent:</code> Action to perform on the valid identifier (or null for no action)<br/><br/>**Returns:**<br/>True for valid identifier (and performs action) else false<br/>|
|`StdIdent2(Action<string> actIdent)`|Scan Standard Identifier of the form: (letter \| _)\* (letterordigit \| _ \| -)\*<br/>- Then perform an action on the identifier if valid.<br/><br/>**Parameters:**<br/><code>actIdent:</code> Action to perform on the valid identifier (or null for no action)<br/><br/>**Returns:**<br/>True for valid identifier (and performs action) else false<br/>|
|`StringDelim()`|Check if current character is a string delimiter (as set in FexScanner)<br/>|
|`StrLit()`|Scan a delimited String Literal:<br/>- Current Index must be at the starting delimiter ("\`' etc)<br/>- Token contains the string (excluding delimiters)<br/><br/>**Returns:**<br/>True: if there was a string literal and Index positioned after ending delimiter<br/>False: for no string or Eos - Index unchanged<br/>|
|`ValueOrStrLit(string termChars, bool orToEos = false)`|Scan either a StrLit or result of ScanTo(termChars, orToEos)<br/>- If Index is at a StringDelim - returns the result of StrLit()<br/>- Else returns the result of ScanTo(termChars, orToEos)<br/>- Token contains the value<br/><br/>**Returns:**<br/>Success of the scan<br/>|
|**Whitespace and Comment Skipping:**||
|`OptSp(string spaceChars = " \t")`|Optionally skip given space characters (default = " \t") - creates optional (Opt) Op<br/>|
|`SkipWS(string wsChars = " \r\n\t", bool opt = false)`|Skip given White Space characters (default: " \r\n\t")<br/><br/>**Parameters:**<br/><code>opt:</code> Make the Op optional or not<br/><br/>**Returns:**<br/>True if not at Eos after skipping or opt == true else False<br/>|
|`SkipWSC(bool termNL = false, string spaceChars = " \t", bool opt = false)`|Skip given space characters (default = " \t"), newlines (if termNL = false) and comments //... or /\*..\*/ (handles nested comments)<br/>- White space: spaceChars + "\r\n" if termNL is false<br/>- Set termNL to position Index at the next newline not inside a block comment (/\*..\*/), else the newlines are also skipped.<br/><br/>**Parameters:**<br/><code>opt:</code> Make the Op optional or not<br/><br/>**Returns:**<br/>True: Whitespace and comments skipped and Index directly after, or no comment error and opt == true<br/>  False: Eos or comment error (bad comment error is Logged. Use IsScanError() to check) - Index unchanged<br/>|
|`Sp(string spaceChars = " \t")`|Skip given space characters (default = " \t")<br/><br/>**Returns:**<br/>True if not at Eos after skipping else False<br/>|
|**Error Messages:**||
|`OnFail(string errorMsg, string errorSource = "Parse error")`|For convenience, bind OnFail to ErrorLog<br/>|
|`Fail(string errorMsg, string errorSource = "Parse error")`|For convenience, bind Fail to ErrorLog<br/>|
