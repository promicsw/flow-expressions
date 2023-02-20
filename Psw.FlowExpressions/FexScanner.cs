// -----------------------------------------------------------------------------
// Copyright (c) Promic Software. All rights reserved.
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
    }
}
