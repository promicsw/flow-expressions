// -----------------------------------------------------------------------------
// Copyright (c) Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

namespace Psw.FlowExpressions
{
    /// <summary>
    /// Use as a context if there is none
    /// </summary>
    public class FexNoContext { }

    /// <summary>
    /// Build and return FexExlements that operate on a context T and can be used in 
    /// other productions or as the Axiom (root) to be run.<br/>
    /// - The innards of each element are built via the encapsulated FexBuilder class
    /// </summary>
    public class FlowExpression<T>
    {
        protected FexBuilder<T> _fexBuilder = new FexBuilder<T>();

        /// <summary>
        /// Define a Sequence (of steps) that must complete in full to pass
        /// </summary>
        public FexElement<T> Seq(Action<FexBuilder<T>> buildFex) => _fexBuilder._Prod(new FexSequence<T>(), buildFex);

        /// <summary>
        /// Define an Optional Sequence. If the initial step passes the remaining sequence must complete in full.<br/>
        /// Note: The initial step(s) may also be Optional and if any of them or the first non-optional step passes 
        /// then the remainder must pass as before.
        /// </summary>
        public FexElement<T> Opt(Action<FexBuilder<T>> buildFex) => _fexBuilder._Prod(new FexOptional<T>(), buildFex);

        /// <summary>
        /// Define a set of Sequences, where one of the sequences must pass
        /// </summary>
        public FexElement<T> OneOf(Action<FexBuilder<T>> buildFex) => _fexBuilder._Prod(new FexOneOf<T>(), buildFex);

        /// <summary>
        /// Define an optional set of sequences where one of them may pass
        /// </summary>
        public FexElement<T> OptOneOf(Action<FexBuilder<T>> buildFex) => Opt(o => OneOf(buildFex));

        /// <summary>
        /// Inverse of OneOf, where it fails if any sequence passes<br />
        /// - Use at the start of Rep.. (repeat loop) to Break out of the loop<br />
        /// - Or at the start of any Opt (optional) sequence to skip the remainder of the sequence
        /// </summary>
        /// <param name="buildFex"></param>
        /// <returns>False if one of the sequences pass else True</returns>
        public FexElement<T> NotOneOf(Action<FexBuilder<T>> buildFex) => _fexBuilder._Prod(new FexNotOneOf<T>(), buildFex);

        /// <summary>
        /// Repeat a sequence repMin up to repMax times (-1 for any reps > repMin) (see documentation for details)
        /// </summary>
        public FexElement<T> Rep(int repMin, int repMax, Action<FexBuilder<T>> buildFex)
            => _fexBuilder._Prod(new FexRepeat<T>(repMin, repMax), buildFex);

        /// <summary>
        /// Repeat sequence repeat times: Equivalent to Rep(repeat, repeat, buildFex)
        /// </summary>
        public FexElement<T> Rep(int repeat, Action<FexBuilder<T>> buildFex) => Rep(repeat, repeat, buildFex);

        /// <summary>
        /// Repeat sequence 0 or more times: Equivalent to Rep(0, -1, buildFex)
        /// </summary>
        public FexElement<T> Rep0N(Action<FexBuilder<T>> buildFex) => Rep(0, -1, buildFex);

        /// <summary>
        /// Repeat sequence 1 or more times: Equivalent to Rep(1, -1, buildFex)
        /// </summary>
        public FexElement<T> Rep1N(Action<FexBuilder<T>> buildFex) => Rep(1, -1, buildFex);

        /// <summary>
        /// Repeat a OneOf construct repMin up to repMax times (-1 for any reps > repMin) (see documentation for details)
        /// </summary>
        public FexElement<T> RepOneOf(int repMin, int repMax, Action<FexBuilder<T>> buildFex) => Rep(repMin, repMax, r => OneOf(buildFex));
    }
}
