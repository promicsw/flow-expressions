// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using Psw.Scanners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psw.FlowExpressions
{
    public class FexScanner : ScriptScanner
    {
        public FexScanner(string source = "", ScanErrorLog errorLog = null) : base(source, errorLog) { }

        /// <summary>
        /// Set comment configuration:<br/>
        /// - For block comments Start and End must both be valid to enable block comments. 
        /// </summary>
        /// <param name="lineComment">Line comment (null/empty for none).</param>
        /// <param name="blockCommentStart">Block comment start (null/empty for none).</param>
        /// <param name="blockCommentEnd">Block comment end (null/empty for none).</param>
        /// <returns>FexScanner for fluent chaining.</returns>
        public FexScanner ConfigComment(string lineComment = "//", string blockCommentStart = "/*", string blockCommentEnd = "*/") {
            SetScriptComment(lineComment, blockCommentStart, blockCommentEnd);
            return this; ;
        }
    }
}
