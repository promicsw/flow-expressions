﻿// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void SetAction(Action<T> preOp) => _preOp = preOp;

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
        /// Assign a name to the current production sequence for later reference
        /// </summary>
        public FexBuilder<T> RefName(string name) {
            _buildState.LogRef(name);
            return this;
        }

        /// <summary>
        /// Reference/include a named sequence in the current sequence
        /// </summary>
        public FexBuilder<T> Ref(string refName) {
            _buildState.LinkRef(refName);
            return this;
        }

        /// <summary>
        /// Optional recursive inclusion of the current production sequence within itself 
        /// </summary>
        public FexBuilder<T> OptSelf() {
            var fex = _buildState.HostFex;
            return Opt(o => o.Fex(fex));
        }

        /// <summary>
        /// Include a set of previously defined production sequences
        /// </summary>
        public FexBuilder<T> Fex(params FexElement<T>[] fex) {
            for (int i = 0; i < fex.Length; i++) AddFex(fex[i]);
            return this;
        }

        /// <summary>
        /// Perform an Operation on Context and current Value returning true/false for pass/failure
        /// </summary>
        public FexBuilder<T> Op(Func<T, FexOpValue, bool> op)
            => AddFex(_buildState.LastFex = _buildState.LastOpr = new FexOpr<T>(op, _buildState.PreOp));

        /// <summary>
        /// Perform an Operation on Context returning true/false for pass/failure
        /// </summary>
        public FexBuilder<T> Op(Func<T, bool> op) => Op((c, v) => op(c));

        /// <summary>
        /// Perform an Operation on Context and always returns true/pass<br/>
        /// - Typically use as the last element of a OneOf set as the default (and valid) action
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public FexBuilder<T> ValidOp(Action<T> action) => Op((c, v) => { action(c); return true; });

        /// <summary>
        /// Assert if a condition is true. Returns false and performs failAction on failure
        /// </summary>
        public FexBuilder<T> Assert(Func<T, bool> assert, Action<T> failAction = null) {
            AddFex(_buildState.LastFex = new FexOpr<T>(assert, _buildState.PreOp));
            _buildState.LastFex.SetFailAction(failAction);
            return this;
        }

        /// <summary>
        /// Perform an Action on the Context (has no affect on a sequence so it can be used at the start of a sequence or loop etc.)
        /// </summary>
        public FexBuilder<T> Act(Action<T> action) => AddFex(new FexAct<T>(action));

        /// <summary>
        /// Perform a repeated Action on the Context (has no affect on a sequence so it can be used at the start of a sequence or loop etc.)<br/>
        /// - The action delegate parameters are the Context and a 0 based index for every iteration.
        /// </summary>
        /// <param name="repeat">Repeat count</param>
        public FexBuilder<T> RepAct(int repeat, Action<T, int> action) => AddFex(new FexRepAct<T>(repeat, action));

        /// <summary>
        /// Perform an Action on the Context and always returns a pass (equivalent to an Op that always returns true).<br/>
        /// - Typically used as the last item in a OneOf construct to make it valid by performing a default action.<br/>
        /// - Using Act in the above case would cause the OneOf construct to fail since Act does not return true;
        /// </summary>
        public FexBuilder<T> DefaultAct(Action<T> action) => AddFex(new FexAct<T>(action, FexCheckResult.Passed));

        /// <summary>
        /// Define a Sequence (of steps) that must complete in full to pass
        /// </summary>
        public FexBuilder<T> Seq(Action<FexBuilder<T>> buildFex) => AddAndBuild(buildFex, new FexSequence<T>());

        /// <summary>
        /// Define an Optional Sequence. If the initial step passes the remaining sequence must complete in full.<br/>
        /// Note: The initial step(s) may also be Optional and if any of them or the first non-optional step passes 
        /// then the remainder must pass as before.
        /// </summary>
        public FexBuilder<T> Opt(Action<FexBuilder<T>> buildFex) => AddAndBuild(buildFex, new FexOptional<T>());

        /// <summary>
        /// Define a set of Sequences, where one of the sequences must pass
        /// </summary>
        /// <param name="buildFex"></param>
        /// <returns>True if one of the sequences pass else False</returns>
        public FexBuilder<T> OneOf(Action<FexBuilder<T>> buildFex) => AddAndBuild(buildFex, new FexOneOf<T>(), true);

        /// <summary>
        /// Define an optional set of sequences where one of them may pass
        /// </summary>
        public FexBuilder<T> OptOneOf(Action<FexBuilder<T>> buildFex) => Opt(o => o.OneOf(buildFex));

        /// <summary>
        /// Inverse of OneOf, where it fails if any sequence passes<br />
        /// - Use at the start of Rep.. (repeat loop) to Break out of the loop <br />
        /// - Or at the start of any Opt (optional) sequence to skip the remainder of the sequence
        /// </summary>
        /// <param name="buildFex"></param>
        /// <returns>False if one of the sequences pass else True</returns>
        public FexBuilder<T> NotOneOf(Action<FexBuilder<T>> buildFex) => AddAndBuild(buildFex, new FexNotOneOf<T>());

        /// <summary>
        /// Alias for NotOneOf - just makes it easier to read in repeat loops
        /// Inverse of OneOf, where it fails if any sequence passes<br />
        /// - Use at the start of Rep.. (repeat loop) to Break out of the loop <br />
        /// - Or at the start of any Opt (optional) sequence to skip the remainder of the sequence
        /// </summary>
        /// <param name="buildFex"></param>
        /// <returns>False if one of the sequences pass else True</returns>
        public FexBuilder<T> BreakOn(Action<FexBuilder<T>> buildFex) => AddAndBuild(buildFex, new FexNotOneOf<T>());

        /// <summary>
        /// Repeat a production repMin up to repMax times (-1 for any reps > repMin) (see documentation for details)
        /// </summary>
        public FexBuilder<T> Rep(int repMin, int repMax, Action<FexBuilder<T>> buildFex)
            => AddAndBuild(buildFex, new FexRepeat<T>(repMin, repMax), true);

        /// <summary>
        /// Repeat sequence repeat times: Equivalent to Rep(repeat, repeat, buildFex)
        /// </summary>
        public FexBuilder<T> Rep(int repeat, Action<FexBuilder<T>> buildFex) => Rep(repeat, repeat, buildFex);

        /// <summary>
        /// Repeat sequence 0 or more times: Equivalent to Rep(0, -1, buildFex)
        /// </summary>
        public FexBuilder<T> Rep0N(Action<FexBuilder<T>> buildFex) => Rep(0, -1, buildFex);

        /// <summary>
        /// Repeat sequence 1 or more times: Equivalent to Rep(1, -1, buildFex)
        /// </summary>
        public FexBuilder<T> Rep1N(Action<FexBuilder<T>> buildFex) => Rep(1, -1, buildFex);

        /// <summary>
        /// Repeat a OneOf construct repMin up to repMax times (-1 for any reps > repMin) (see documentation for details)
        /// </summary>
        public FexBuilder<T> RepOneOf(int repMin, int repMax, Action<FexBuilder<T>> buildFex)
            => Rep(repMin, repMax, r => r.OneOf(buildFex));

        /// <summary>
        /// Action to perform if a production fails:<br/>
        /// o Valid after an Op, OneOf or Rep, else it is ignored
        /// </summary>
        public FexBuilder<T> OnFail(Action<T> failAction) {
            _buildState.LastFex?.SetFailAction(failAction);
            return this;
        }

        /// <summary>
        /// Force a Fail Operation - can use as last operation in a OneOf set
        /// </summary>
        public FexBuilder<T> Fail(Action<T> failAction) => AddFex(new FexFail<T>(failAction));

        /// <summary>
        /// Perform an action on the value of the last Operator, if the value is not null<br/>
        /// o The value must be cast to the appropriate type
        /// </summary>
        public FexBuilder<T> ActValue<V>(Action<V> valueAction) {
            if (valueAction != null) _buildState.LastOpr?.SetValueAction(o => valueAction((V)o));
            return this;
        }

        public FexBuilder<T> GlobalPreOp(Action<T> preOp) {
            _buildState.PreOp.SetAction(preOp);
            return this;
        }


        /// <summary>
        /// Set/override the PreOp for the preceding Op
        /// </summary>
        public FexBuilder<T> PreOp(Action<T> preOp) {
            _buildState.LastOpr?.SetPreOp(new FexPreOp<T>(preOp));
            return this;
        }

        protected bool _traceOn = true;

        /// <summary>
        /// Switch Tracing on or off
        /// </summary>
        public FexBuilder<T> TraceOn(bool on = true) {
            _traceOn = on;
            return this;
        }

        /// <summary>
        /// Trace Action to perform on last Op
        /// </summary>
        public FexBuilder<T> Trace(Action<T, bool> traceAction) {
            if (_traceOn) _buildState.LastOpr?.SetTraceAction((c, v, r) => traceAction(c, r));
            return this;
        }

        /// <summary>
        /// Trace Action to perform on last Op, including access to the Op's value
        /// </summary>
        public FexBuilder<T> Trace(Action<T, object, bool> traceAction) {
            if (_traceOn) _buildState.LastOpr?.SetTraceAction(traceAction);
            return this;
        }
    }
}
