// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Psw.FlowExpressions
{
    /// <summary>
    /// PreOperator management class
    /// </summary>
    public class FexPreOp<T>
    {
        private Action<T> _preOp;
        private bool _hasRun = false;

        public FexPreOp(Action<T> preOp) => _preOp = preOp;

        public void SetAction(Action<T>? preOp) => _preOp = preOp;

        public void Run(T ctx) {
            if (!_hasRun) _preOp?.Invoke(ctx);
            _hasRun = true;
        }

        public void Reset() => _hasRun = false;
    }

    /// <summary>
    /// Fluent Flow Expression Builder
    /// </summary>
    public class FexBuilder<T>
    {
        protected FexBuildState<T> _buildState;

        public FexBuilder(FexBuildState<T> buildState) => _buildState = buildState;

        protected FexBuilder<T> AddFex(FexElement<T> element) {
            _buildState.HostFex.Add(element); 
            return this;
        }

        protected FexBuilder<T> AddAndBuild(Action<FexBuilder<T>> build, FexElement<T> prod, bool setLastFex = false)
            => _buildState.AddAndBuild(this, build, prod, setLastFex);

        /// <summary>
        /// Assign a name to the current production sequence for later reference:<br/>
        /// - Note: a later lookup of this reference is not case sensitive.
        /// </summary>
        public FexBuilder<T> RefName(string name) {
            _buildState.LogRef(name.ToLower());
            return this;
        }

        /// <summary>
        /// Reference/include a named sequence in the current sequence:<br/>
        /// - Note: the refName lookup is not case sensitive.
        /// </summary>
        public FexBuilder<T> Ref(string refName) {
            _buildState.LinkRef(refName.ToLower());
            return this;
        }

        /// <summary>
        /// Optional recursive inclusion of the current production sequence within itself. 
        /// </summary>
        public FexBuilder<T> OptSelf() {
            var fex = _buildState.HostFex;
            return Opt(o => o.Fex(fex));
        }

        /// <summary>
        /// Include a set of previously defined FexElements (sub-expressions).
        /// </summary>
        public FexBuilder<T> Fex(params FexElement<T>[] fex) {
            for (int i = 0; i < fex.Length; i++) AddFex(fex[i]);
            return this;
        }

        /// <summary>
        /// Perform an Operation on the Context-T and current Value (for operators that record a value) returning a boolean for pass/failure.
        /// </summary>
        public FexBuilder<T> Op(Func<T, FexOpValue, bool> op)
            => AddFex(_buildState.LastFex = _buildState.LastOpr = new FexOpr<T>(op, _buildState.PreOp));

        /// <summary>
        /// Perform an Operation on the Context-T returning a boolean for pass/failure.
        /// </summary>
        public FexBuilder<T> Op(Func<T, bool> op) => Op((c, v) => op(c));

        /// <summary>
        /// Perform an Operation on the Context-T and always returns true/pass:<br/>
        /// - Typically use as the last element of a OneOf set as the default (and valid) action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public FexBuilder<T> ValidOp(Action<T> action) => Op((c, v) => { action(c); return true; });

        /// <summary>
        /// Assert if a condition on the Context-T is true. Returns false and performs failAction on the Context-T for a failure.
        /// </summary>
        public FexBuilder<T> Assert(Func<T, bool> assert, Action<T> failAction = null) {
            AddFex(_buildState.LastFex = new FexOpr<T>(assert, _buildState.PreOp));
            _buildState.LastFex.SetFailAction(failAction);
            return this;
        }

        /// <summary>
        /// Perform an Action on the Context-T (does no affect the validity of a sequence).
        /// </summary>
        public FexBuilder<T> Act(Action<T>? action) => AddFex(new FexAct<T>(action));

        /// <summary>
        /// Perform a repeated Action on the Context-T (does no affect the validity of a sequence):<br/>
        /// - The action delegate parameters are the Context-T and a 0 based iteration counter.
        /// </summary>
        /// <param name="repeat">Repeat count</param>
        public FexBuilder<T> RepAct(int repeat, Action<T, int> action) => AddFex(new FexRepAct<T>(repeat, action));

        /// <summary>
        /// Perform an Action on the Context and always returns a pass (equivalent to an Op that always returns true).<br/>
        /// - Typically used as the last item in a OneOf construct to make it valid by performing a default action.<br/>
        /// - Using Act in the above case would cause the OneOf construct to fail since Act does not return true;
        /// </summary>
        //Replaced by ValidOp
        //public FexBuilder<T> DefaultAct(Action<T> action) => AddFex(new FexAct<T>(action, FexCheckResult.Passed));

        /// <summary>
        /// Define a Sequence (of steps) that must complete in full to pass:<br/>
        /// - A step is any FexElement.
        /// </summary>
        public FexBuilder<T> Seq(Action<FexBuilder<T>> buildFex) => AddAndBuild(buildFex, new FexSequence<T>());

        /// <summary>
        /// Define an Optional Sequence:<br/>
        /// - If the first step passes the remainder must succeed.<br/>
        /// - If the first step fails the remainder is aborted - without error.<br/>
        /// - Note: The first step(s) may themselves be Optional and if any of them or the first non-optional step passes 
        /// then the remainder must pass as before.
        /// </summary>
        public FexBuilder<T> Opt(Action<FexBuilder<T>> buildFex) => AddAndBuild(buildFex, new FexOptional<T>());

        /// <summary>
        /// Define a set of Sequences, where one of the sequences must succeed:<br/>
        /// - Execution breaks out at the point where it succeeds.
        /// - If none of the sequences pass then the production fails.
        /// </summary>
        public FexBuilder<T> OneOf(Action<FexBuilder<T>> buildFex) => AddAndBuild(buildFex, new FexOneOf<T>(), true);

        /// <summary>
        /// Define an optional set of sequences where one of them may pass:<br/>
        /// - Same as OneOf but does not fail if no sequence passes.
        /// - Execution breaks out at the point where it does succeeds.
        /// </summary>
        public FexBuilder<T> OptOneOf(Action<FexBuilder<T>> buildFex) => Opt(o => o.OneOf(buildFex));

        /// <summary>
        /// Define a set of Sequences, where it fails if any sequence passes (inverse of OneOf):<br/>
        /// - Typically used at the beginning of Rep(eat) loops to break out of the loop.<br/>
        /// - Or at the start of any Opt (optional) sequence to skip the remainder of the sequence.<br/>
        /// - Execution short circuits if any sequence succeeds.<br/>
        /// - If any of the sequences pass then the production fails.
        /// </summary>
        public FexBuilder<T> NotOneOf(Action<FexBuilder<T>> buildFex) => AddAndBuild(buildFex, new FexNotOneOf<T>());

        /// <summary>
        /// Alias for NotOneOf - just makes it easier to read in repeat loops.<br/><br/>
        /// Define a set of Sequences, where it fails if any sequence passes (inverse of OneOf):<br/>
        /// - Typically used at the beginning of Rep(eat) loops to break out of the loop.<br/>
        /// - Or at the start of any Opt (optional) sequence to skip the remainder of the sequence.<br/>
        /// - Execution short circuits if any sequence succeeds.<br/>
        /// - If any of the sequences pass then the production fails.
        /// </summary>
        public FexBuilder<T> BreakOn(Action<FexBuilder<T>> buildFex) => AddAndBuild(buildFex, new FexNotOneOf<T>());

        /// <summary>
        /// Defines a repeated sequence and the following rules apply:<br/>
        /// - repMin = 0: Repeat 0 to repMax times.Treats the sequence as an optional(see Opt rules).<br/>
        /// - repMin > 0: Must repeat at least repMin times.<br/>
        /// - repMax = -1: Repeat repMin to N times.Treats the sequence, after repMin, as an optional (see Opt rules).<br/>
        /// - repMax > 0: Repeat repMin to repMax times and then terminates the loop.
        /// </summary>
        public FexBuilder<T> Rep(int repMin, int repMax, Action<FexBuilder<T>> buildFex)
            => AddAndBuild(buildFex, new FexRepeat<T>(repMin, repMax), true);

        /// <summary>
        /// Repeat a sequence repCount times - equivalent to Rep(repCount, repCount, buildFex):<br/>
        /// - The sequence must repeat exactly repCount times else the production fails.
        /// </summary>
        public FexBuilder<T> Rep(int repCount, Action<FexBuilder<T>> buildFex) => Rep(repCount, repCount, buildFex);

        /// <summary>
        /// Repeat sequence 0 or more times - equivalent to Rep(0, -1, buildFex):<br/>
        /// - Treats the sequence as an optional.<br/>
        /// - Keeps repeating the sequence until it fails.
        /// </summary>
        public FexBuilder<T> Rep0N(Action<FexBuilder<T>> buildFex) => Rep(0, -1, buildFex);

        /// <summary>
        /// Repeat sequence one or more times - equivalent to Rep(1, -1, buildFex):<br/>
        /// - The sequence must succeed at least once else the production fails.<br/>
        /// - Then keeps repeating the sequence until it fails.
        /// </summary>
        public FexBuilder<T> Rep1N(Action<FexBuilder<T>> buildFex) => Rep(1, -1, buildFex);

        /// <summary>
        /// Repeat a OneOf expression and the following rules apply:<br/>
        /// - repMin = 0: Repeat 0 to repMax times. Treats the OneOf as an optional(see Opt rules).<br/>
        /// - repMin > 0: Must repeat at least repMin times.<br/>
        /// - repMax = -1: Repeat repMin to N times.Treats the OneOf, after repMin, as an optional (see Opt rules).<br/>
        /// - repMax > 0: Repeat repMin to repMax times and then terminates the loop.
        /// </summary>
        public FexBuilder<T> RepOneOf(int repMin, int repMax, Action<FexBuilder<T>> buildFex)
            => Rep(repMin, repMax, r => r.OneOf(buildFex));

        /// <summary>
        /// Action to perform if the last Op (or derivative), Rep or OneOf failed:<br/>
        /// - Typically used for error reporting.<br/>
        /// - Valid only after an Op, OneOf or Rep, else it is ignored.<br/>
        /// - Invoked only if the last Op, Rep or OneOf failed.<br/>
        /// </summary>
        public FexBuilder<T> OnFail(Action<T> failAction) {
            _buildState.LastFex?.SetFailAction(failAction);
            return this;
        }

        /// <summary>
        /// Force a fail condition and performs the failAction.
        /// </summary>
        public FexBuilder<T> Fail(Action<T> failAction) => AddFex(new FexFail<T>(failAction));

        /// <summary>
        /// Binds an Action to an operator (Op) that recorded a value, and should follow directly after the Op:<br/>
        /// - If the Op succeeds, and has a non-null value, then valueAction is invoked.<br/>
        /// - The value is recorded as an object and must be cast to the actual type before use (via V, or it may be inferred from the action).
        /// </summary>
        public FexBuilder<T> ActValue<V>(Action<V> valueAction) {
            if (valueAction != null) _buildState.LastOpr?.SetValueAction(o => valueAction((V)o));
            return this;
        }

        /// <summary>
        /// Perform the previously defined Skip action (define via FlowExpression.DefSkip(...)):<br/>
        /// - Typically used to skip spaces/white-space etc.<br/>
        /// - Runs, if defined, as an action and does no affect the validity of a sequence.
        /// </summary>
        public FexBuilder<T> Skip() => Act(_buildState.SkipOp);


        /// <summary>
        /// Binds a pre-operator to all subsequent operators:<br/>
        /// - A pre-operator executes before an operator as an action on the context:<br/>
        /// - Typically used to skip spaces etc. before tokens when parsing.<br/>
        /// - Pre-operators are efficient an only execute once while trying several lookahead operations.
        /// </summary>
        public FexBuilder<T> GlobalPreOp(Action<T>? preOp) {
            _buildState.PreOp.SetAction(preOp);
            return this;
        }

        /// <summary>
        /// Binds the previously defined Skip action (define via FlowExpression.DefSkip(...)) as pre-operator to all subsequent operators:<br/>
        /// - A pre-operator executes before an operator as an action on the context:<br/>
        /// - Typically used to skip spaces/white-space etc. before tokens when parsing.<br/>
        /// - Pre-operators are efficient an only execute once while trying several lookahead operations.
        /// </summary>
        public FexBuilder<T> GlobalSkip() => GlobalPreOp(_buildState.SkipOp);


        /// <summary>
        /// Bind a pre-operator to the preceding operator:<br/>
        /// - A pre-operator executes before an operator as an action on the context.<br/>
        /// - Typically used to skip spaces etc. before tokens when parsing.<br/>
        /// - Pre-operators are efficient an only execute once while trying several lookahead operations.<br/>
        /// - The preOp may be null if no PreOp should be executed.<br/>
        /// - The above mechanism could be used to switch off the GlobalPreOp's for selected Op's.
        /// </summary>
        public FexBuilder<T> PreOp(Action<T> preOp) {
            _buildState.LastOpr?.SetPreOp(new FexPreOp<T>(preOp));
            return this;
        }

        /// <summary>
        /// Produce a general Trace message via the context and assign an optional level:<br/>
        /// - Note: Enable tracing by passing an IFexTracer to the FlowExpression constructor, else this has no effect.<br/>
        /// - Calls: `IFexTracer.Trace(traceMessage, level)`
        /// </summary>
        public FexBuilder<T> Trace(Func<T, string> traceMessage, int level = 0) {
            if (_buildState.TraceOn) AddFex(new FexAct<T>(c => _buildState.Tracer.Trace(traceMessage(c), level)));
            return this;
        }

        /// <summary>
        /// Bind a Trace message to the preceding operator:<br/>
        /// - Produce a Trace message via the context and assign an optional level.<br/>
        /// - Note: Enable tracing by passing an IFexTracer to the FlowExpression constructor, else this has no effect.<br/>
        /// - Calls: `IFexTracer.Trace(traceMessage, pass, level)` where pass is the result of the preceding operator.
        /// </summary>
        public FexBuilder<T> TraceOp(Func<T, string> traceMessage, int level = 0) {
            if (_buildState.TraceOn) _buildState.LastOpr?.SetTraceAction((c, v, r) => _buildState.Tracer.Trace(traceMessage(c), r, level));
            return this;
        }

        /// <summary>
        /// Bind a Trace message to the preceding operator that produces a value:<br/>
        /// - Produce a Trace message via the context, value (as an object) and assign an optional level.<br/>
        /// - Note: Enable tracing by passing an IFexTracer to the FlowExpression constructor, else this has no effect.<br/>
        /// - Calls: `IFexTracer.Trace(traceMessage, pass, level)` where pass is the result of the preceding operator.
        /// </summary>
        public FexBuilder<T> TraceOp(Func<T, object, string> traceMessage, int level = 0) {
            if (_buildState.TraceOn) _buildState.LastOpr?.SetTraceAction((c, v, r) => _buildState.Tracer.Trace(traceMessage(c, v), r, level));
            return this;
        }

    }
}
