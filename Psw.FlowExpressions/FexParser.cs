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
    /// <summary>
    /// Convenience class for building and running a Parser based on FexScanner<br/>
    /// - Create intermediate FexElements that my be used in other productions<br/>
    /// - Create the Axiom (root) FexElement that may then be Run.
    /// </summary>
    /// <mdoc>
    /// > Essentially wraps `FlowExpression<FexScanner>` and creates or uses a provided FexScanner as the context
    /// </mdoc>
    public class FexParser : FlowExpression<FexScanner>
    {
        protected FexScanner _scn;

        /// <summary>
        /// Construct with given FexScanner
        /// </summary>
        public FexParser(FexScanner scn) => _scn = scn;

        /// <summary>
        /// Construct with given source and optional ScanErroLog.<br/>
        /// - Creates the FexScanner with source and optional, else default internal, ScanErrorLog 
        /// </summary>
        public FexParser(string source, ScanErrorLog errorLog = null) => _scn = new FexScanner(source, errorLog);

        /// <summary>
        /// Run the Axion (root) FexElement
        /// </summary>
        /// <param name="axiom">Axiom FexElement to run </param>
        /// <param name="passOutput">Text to return if it passed</param>
        /// <param name="failOutput">Test to return if it failed. Can use the supplied ScanErrorLog to format the output</param>
        /// <returns>Result string</returns>
        public string Run(FexElement<FexScanner> axiom, Func<string> passOutput = null, Func<ScanErrorLog, string> failOutput = null)
           => axiom.Run(_scn) ? passOutput?.Invoke() ?? "Passed"
                              : failOutput?.Invoke(_scn.ErrorLog) ?? "Failed";
    }
}
