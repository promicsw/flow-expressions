// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psw.FlowExpressions
{
    /// <summary>
    /// Interface used by the Flow Expressions tracing facilities:<br/>
    /// - To enable tracing pass an implementation to the FlowExpression constructor.<br/>
    /// - An implementation could produce console output, write to a file etc. 
    /// </summary>
    public interface IFexTracer
    {
        /// <summary>
        /// For general tracing messages that could be used anywhere
        /// </summary>
        /// <param name="message">The trace message</param>
        /// <param name="level">A level indicator that can be used by the implementation</param>
        void Trace(string message, int level);

        /// <summary>
        /// For trace messages that are bound to the preceding operator.
        /// </summary>
        /// <param name="message">Trace message</param>
        /// <param name="pass">Result (pass/fail) of the preceding operator</param>
        /// <param name="level">A level indicator that can be used by the implementation</param>
        void Trace(string message, bool pass, int level);
    }

}
