// -----------------------------------------------------------------------------
// Copyright (c) Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

namespace Psw.FlowExpressions
{
    public enum FexCheckResult { Passed, FailFirst, FailRemainder }

    /// <summary>
    /// Fex Element base class
    /// </summary>
    /// <typeparam name="T">Flow Expression Context</typeparam>
    public class FexElement<T>
    {
        protected Action<T>? _failAction = null;

        protected void RunFailAction(T ctx) => _failAction?.Invoke(ctx);

        public void SetFailAction(Action<T> failAction) => _failAction = failAction;

        public virtual bool Run(T ctx) => false;

        /// <summary>
        /// Checks first step and then runs remainder if valid.<br/>
        /// - For the base element there is only one step
        /// </summary>
        /// <returns>
        /// FexCheckResult.Passed : All Passed or there are no steps<br/>
        /// FexCheckResult.FailFirst : First step failed<br/>
        /// FexCheckResult.FailRemainder : First step passed but remainder failed
        /// </returns>
        public virtual FexCheckResult CheckRun(T ctx) => Run(ctx) ? FexCheckResult.Passed : FexCheckResult.FailFirst;

        public virtual void Add(FexElement<T> exp) { }

        public bool IsOpt { get; set; }

    }

    /// <summary>
    /// Perform an Action and force a failure
    /// </summary>
    public class FexFail<T> : FexElement<T>
    {
        public FexFail(Action<T> failAction) => SetFailAction(failAction);

        public override bool Run(T ctx) {
            RunFailAction(ctx);
            return false;
        }

        public override FexCheckResult CheckRun(T ctx) {
            RunFailAction(ctx);
            return FexCheckResult.FailRemainder;
        }
    }

    /// <summary>
    /// Perform an Action on the (operates as FailFirst optional so won't affect a sequence)
    /// </summary>
    public class FexAct<T> : FexElement<T>
    {
        private readonly Action<T> _action;
        private readonly FexCheckResult _checkRunReturn;

        public FexAct(Action<T> action, FexCheckResult checkRunReturn = FexCheckResult.FailFirst) {
            _action = action;
            _checkRunReturn = checkRunReturn;
            IsOpt = true;
        }

        public override bool Run(T ctx) {
            _action?.Invoke(ctx);
            return true;
        }

        public override FexCheckResult CheckRun(T ctx) {
            _action?.Invoke(ctx);
            return _checkRunReturn;
        }
    }

    public class FexRepAct<T> : FexElement<T>
    {
        private readonly Action<T, int> _action;
        private readonly int _repeat;

        public FexRepAct(int repeat, Action<T, int> action) {
            _action = action;
            _repeat = repeat;
            IsOpt = true;
        }

        public override bool Run(T ctx) {
            for (int i = 0; i < _repeat; i++) _action?.Invoke(ctx, i);
            return true;
        }

        public override FexCheckResult CheckRun(T ctx) {
            Run(ctx);
            return FexCheckResult.FailFirst;
        }
    }

    /// <summary>
    /// Reference a named element
    /// </summary>
    public class FexRef<T> : FexElement<T>
    {
        public FexElement<T> Ref = new FexElement<T>();

        public override bool Run(T ctx) => Ref.Run(ctx);
        public override FexCheckResult CheckRun(T ctx) => Ref.CheckRun(ctx);
    }

    /// <summary>
    /// Op Value management class
    /// </summary>
    public class FexOpValue
    {
        public object? Value = null;

        // Set Value and returns true if res is true
        // Else Value is set to null and returns false
        public bool SetValue(bool res, object value) {
            Value = res ? value : null;
            return res;
        }
    }

    /// <summary>
    /// Operator: Performs and action on Context and returns success result
    /// </summary>
    public class FexOpr<T> : FexElement<T>
    {
        private Func<T, FexOpValue, bool>? _opr;
        private FexPreOp<T>? _preOp;
        private Action<object>? _valueAction = null;
        private Action<T, object, bool>? _traceAction = null;

        public FexOpr(Func<T, FexOpValue, bool> opr, FexPreOp<T> preOp) { _opr = opr; _preOp = preOp; }

        public FexOpr(Func<T, bool> opr, FexPreOp<T> preOp) {
            _opr = (c, v) => opr(c);
            _preOp = preOp;
        }

        public void SetPreOp(FexPreOp<T>? preOp) => _preOp = preOp;

        public void SetValueAction(Action<object> valueAction) => _valueAction = valueAction;

        public void SetTraceAction(Action<T, object, bool> traceAction) => _traceAction = traceAction;

        private bool _Run(T ctx, bool checkMode) {
            _preOp.Run(ctx);

            var v = new FexOpValue();
            var res = _opr?.Invoke(ctx, v) ?? false;

            _traceAction?.Invoke(ctx, v.Value, res);

            if (res) {
                _preOp.Reset();
                if (v.Value != null) _valueAction?.Invoke(v.Value); // ToDo: Check - null might be a valid value
            }
            else if (!checkMode) RunFailAction(ctx);

            return res;
        }

        public override bool Run(T ctx) => _Run(ctx, false);

        public override FexCheckResult CheckRun(T ctx) => _Run(ctx, true) ? FexCheckResult.Passed : FexCheckResult.FailFirst;
    }

    /// <summary>
    /// Construct, Run and Manage a Sequence of steps (FexElements)
    /// </summary>
    public class FexSequence<T> : FexElement<T>
    {
        protected List<FexElement<T>> steps = new List<FexElement<T>>();

        public override void Add(FexElement<T> exp) => steps.Add(exp);

        public override bool Run(T ctx) => RunSteps(ctx);

        public bool RunSteps(T ctx) {
            foreach (var step in steps) if (!step.Run(ctx)) return false;
            return true;
        }

        /// <summary>
        /// Checks first step and then runs remainder if valid.
        /// </summary>
        /// <returns>
        /// FexCheckResult.Passed : All Passed or no steps<br/>
        /// FexCheckResult.FailFirst : First step failed<br/>
        /// FexCheckResult.FailRemainder : First step passed but remainder failed
        /// </returns>
        public override FexCheckResult CheckRun(T ctx) {
            int i = 0;

            // Check if initial step passes (= lookahead):
            // o Skip initial Optionals if they fail
            while (i < steps.Count) {
                var res = steps[i].CheckRun(ctx);
                if (res == FexCheckResult.Passed) break;
                if (res == FexCheckResult.FailFirst && steps[i].IsOpt) {
                    i++;
                    continue;
                }

                return res;
            }

            // First non-optional, or an optional passed, so now Remainder must pass
            i++;
            while (i < steps.Count) if (!steps[i++].Run(ctx)) return FexCheckResult.FailRemainder;
            return FexCheckResult.Passed;

        }
    }

    /// <summary>
    /// Repeat a sequence
    /// </summary>
    public class FexRepeat<T> : FexSequence<T>
    {
        private readonly int _repMin, _repMax,    // Default range
                             _crepMin, _crepMax;  // Range to use in CheckRun

        public FexRepeat(int repMin, int repMax) {
            _repMin = repMin < 0 ? 0 : repMin;
            _repMax = repMax > 0 ? repMax > _repMin ? repMax - _repMin : 0 : repMax;

            //if (_repMax > 0) _repMax = _repMax > _repMin ? _repMax - _repMin : 0;
            IsOpt = _repMin == 0;

            _crepMin = _repMin > 1 ? _repMin - 1 : _repMin;
            _crepMax = _repMax > 1 ? _repMax - 1 : _repMax;
        }

        protected bool RepRun(T ctx, int reps) {
            while (0 < reps--) if (!RunSteps(ctx)) { RunFailAction(ctx); return false; }
            return true;
        }

        protected bool RepCheckRun(T ctx, int reps, bool repN = false) {
            while (repN || 0 < reps--) {
                var res = base.CheckRun(ctx);
                if (res == FexCheckResult.FailRemainder) { RunFailAction(ctx); return false; }
                if (res == FexCheckResult.FailFirst) break;
            }
            return true;
        }

        protected bool RangeRun(T ctx, int rMin, int rMax) => RepRun(ctx, rMin) ? RepCheckRun(ctx, rMax, rMax < 0) : false;

        public override bool Run(T ctx) => RangeRun(ctx, _repMin, _repMax);

        public override FexCheckResult CheckRun(T ctx) {
            var res = base.CheckRun(ctx);
            if (res != FexCheckResult.Passed) return res;

            // Run remainder
            return RangeRun(ctx, _crepMin, _crepMax) ? FexCheckResult.Passed : FexCheckResult.FailRemainder;
        }

    }

    /// <summary>
    /// Manage Optional sequence
    /// </summary>
    public class FexOptional<T> : FexSequence<T>
    {
        public FexOptional() => IsOpt = true;

        public override bool Run(T ctx) => CheckRun(ctx) != FexCheckResult.FailRemainder;
    }

    /// <summary>
    /// Manage a OneOf (or) set of sequences
    /// </summary>
    public class FexOneOf<T> : FexSequence<T>
    {
        public override bool Run(T ctx) {
            FexCheckResult res;

            foreach (var step in steps) {
                res = step.CheckRun(ctx);
                if (res == FexCheckResult.Passed) return true;
                if (res == FexCheckResult.FailRemainder) return false;
            }

            RunFailAction(ctx);
            return false;
        }

        /// <summary>
        /// Checks first step in each sequence and then runs remainder of sequence if valid.
        /// </summary>
        /// <returns>
        /// FexCheckResult.Passed : All Passed or no steps<br/>
        /// FexCheckResult.FailFirst : First step of seqence failed<br/>
        /// FexCheckResult.FailRemainder : First step passed but remainder of sequence failed
        /// </returns>
        public override FexCheckResult CheckRun(T ctx) {
            FexCheckResult res;

            foreach (var step in steps) {
                res = step.CheckRun(ctx);
                if (res == FexCheckResult.Passed) return FexCheckResult.Passed;
                if (res == FexCheckResult.FailFirst) continue;
                return res;  // A sequence ran but failed to complete => FexCheckResult.FailRemainder
            }

            return FexCheckResult.FailFirst; // None of the sequences ran, so FailFirst
        }

    }

    /// <summary>
    /// Inverse of FexOneOf
    /// </summary>
    public class FexNotOneOf<T> : FexOneOf<T>
    {
        public override bool Run(T ctx) => !base.Run(ctx);
        public override FexCheckResult CheckRun(T ctx) => base.CheckRun(ctx) == FexCheckResult.Passed ? FexCheckResult.FailFirst : FexCheckResult.Passed;
    }
}