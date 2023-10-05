// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

namespace Psw.FlowExpressions
{
    /// <summary>
    /// Use as a context if there is none
    /// </summary>
    public class FexNoContext { }

    /// <summary>
    /// Build and return FexElements that operate on a Context-T and can be used in 
    /// other productions or as the Axiom (root) to be run.<br/>
    /// - The innards of each element are built via the encapsulated FexBuilder class.
    /// </summary>
    public class FlowExpression<T>
    {
        protected FexBuildState<T> _fexBuildState;
        protected FexBuilder<T> _fexBuilder;

        /// <group>Constructor</group>
        /// <summary>
        /// Class to create FexElements that can be assembled as a flow expression and then run.
        /// </summary>
        /// <param name="tracer">Supply an IFexTracer to enable tracing.</param>
        public FlowExpression(IFexTracer tracer = null)
        {
            _fexBuildState = new FexBuildState<T>();
            _fexBuildState.Tracer = tracer;
            _fexBuilder = new FexBuilder<T>(_fexBuildState);
        }

        protected FexElement<T> _Prod(FexElement<T> prod, Action<FexBuilder<T>> build) 
            => _fexBuildState.Production(_fexBuilder, prod, build);

        /// <summary>
        /// Define a Sequence (of steps) that must complete in full to pass:<br/>
        /// - A step is any FexElement.
        /// </summary>
        public FexElement<T> Seq(Action<FexBuilder<T>> buildFex) => _Prod(new FexSequence<T>(), buildFex);

        /// <summary>
        /// Define an Optional Sequence:<br/>
        /// - If the first step passes the remainder must succeed.<br/>
        /// - If the first step fails the remainder is aborted - without error.<br/>
        /// - Note: The first step(s) may themselves be Optional and if any of them or the first non-optional step passes 
        /// then the remainder must pass as before.
        /// </summary>
        public FexElement<T> Opt(Action<FexBuilder<T>> buildFex) => _Prod(new FexOptional<T>(), buildFex);

        /// <summary>
        /// Define a set of Sequences, where one of the sequences must succeed:<br/>
        /// - Execution breaks out at the point where it succeeds.
        /// - If none of the sequences pass then the production fails.
        /// </summary>
        public FexElement<T> OneOf(Action<FexBuilder<T>> buildFex) => _Prod(new FexOneOf<T>(), buildFex);

        /// <summary>
        /// Define an optional set of sequences where one of them may pass:<br/>
        /// - Same as OneOf but does not fail if no sequence passes.
        /// - Execution breaks out at the point where it does succeeds.
        /// </summary>
        public FexElement<T> OptOneOf(Action<FexBuilder<T>> buildFex) => Opt(o => o.OneOf(buildFex));

        /// <summary>
        /// Define a set of Sequences, where it fails if any sequence passes (inverse of OneOf):<br/>
        /// - Typically used at the beginning of Rep(eat) loops to break out of the loop.<br/>
        /// - Or at the start of any Opt (optional) sequence to skip the remainder of the sequence.<br/>
        /// - Execution short circuits if any sequence succeeds.<br/>
        /// - If any of the sequences pass then the production fails.
        /// </summary>
        public FexElement<T> NotOneOf(Action<FexBuilder<T>> buildFex) => _Prod(new FexNotOneOf<T>(), buildFex);

        /// <summary>
        /// Defines a repeated sequence and the following rules apply:<br/>
        /// - repMin = 0: Repeat 0 to repMax times.Treats the sequence as an optional(see Opt rules).<br/>
        /// - repMin > 0: Must repeat at least repMin times.<br/>
        /// - repMax = -1: Repeat repMin to N times.Treats the sequence, after repMin, as an optional (see Opt rules).<br/>
        /// - repMax > 0: Repeat repMin to repMax times and then terminates the loop.
        /// </summary>
        public FexElement<T> Rep(int repMin, int repMax, Action<FexBuilder<T>> buildFex) => _Prod(new FexRepeat<T>(repMin, repMax), buildFex);

        /// <summary>
        /// Repeat a sequence repCount times - equivalent to Rep(repCount, repCount, buildFex):<br/>
        /// - The sequence must repeat exactly repCount times else the production fails.
        /// </summary>
        public FexElement<T> Rep(int repeat, Action<FexBuilder<T>> buildFex) => Rep(repeat, repeat, buildFex);

        /// <summary>
        /// Repeat sequence 0 or more times - equivalent to Rep(0, -1, buildFex):<br/>
        /// - Treats the sequence as an optional.<br/>
        /// - Keeps repeating the sequence until it fails.
        /// </summary>
        public FexElement<T> Rep0N(Action<FexBuilder<T>> buildFex) => Rep(0, -1, buildFex);

        /// <summary>
        /// Repeat sequence one or more times - equivalent to Rep(1, -1, buildFex):<br/>
        /// - The sequence must succeed at least once else the production fails.<br/>
        /// - Then keeps repeating the sequence until it fails.
        /// </summary>
        public FexElement<T> Rep1N(Action<FexBuilder<T>> buildFex) => Rep(1, -1, buildFex);

        /// <summary>
        /// Repeat a OneOf expression and the following rules apply:<br/>
        /// - repMin = 0: Repeat 0 to repMax times. Treats the OneOf as an optional(see Opt rules).<br/>
        /// - repMin > 0: Must repeat at least repMin times.<br/>
        /// - repMax = -1: Repeat repMin to N times.Treats the OneOf, after repMin, as an optional (see Opt rules).<br/>
        /// - repMax > 0: Repeat repMin to repMax times and then terminates the loop.
        /// </summary>
        public FexElement<T> RepOneOf(int repMin, int repMax, Action<FexBuilder<T>> buildFex) => Rep(repMin, repMax, r => r.OneOf(buildFex));
    }
}
