// -----------------------------------------------------------------------------
// Copyright (c) Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using Psw.FlowExpressions;

namespace FexExampleSet
{
    /// <summary>
    /// Extensions for Console Tracing
    /// </summary>
    public static class FexBuilderExt
    {
        public static FexBuilder<T> CTrace<T>(this FexBuilder<T> b, string text) 
            => b.Trace((c, res) => Console.WriteLine($"{text} : {res}"));

        public static FexBuilder<T> CTrace<T>(this FexBuilder<T> b, Func<T, string> trace) 
            => b.Trace((c, res) => Console.WriteLine($"{trace(c)} : {res}"));

        public static FexBuilder<T> CTrace<T>(this FexBuilder<T> b, Func<T, object, string> trace)
            => b.Trace((c, v, res) => Console.WriteLine($"{trace(c, v)} : {res}"));
    }

}
