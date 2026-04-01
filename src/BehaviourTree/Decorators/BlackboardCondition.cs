using System;
using BehaviourTree.Blackboard;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that checks a blackboard key condition before executing the child.
    /// Supports observer abort: when the condition changes while the child is running,
    /// the child can be automatically aborted.
    /// Implements IAbortableObserver for LowerPriority abort support by parent composites.
    /// </summary>
    /// <typeparam name="TContext">Context type (must provide IBlackboard access)</typeparam>
    public sealed partial class BlackboardCondition<TContext> : DecoratorBehaviour<TContext>, IAbortableObserver
    {
        private readonly Func<TContext, IBlackboard> _getBlackboard;
        private readonly string _key;
        private readonly Func<object?, bool> _condition;
        private readonly AbortMode _abortMode;
        private IDisposable? _observerHandle;
        private bool _conditionChanged;
        private bool _abortRequested;

        /// <summary>
        /// Creates a blackboard condition decorator.
        /// </summary>
        public BlackboardCondition(
            IBehaviour<TContext> child,
            Func<TContext, IBlackboard> getBlackboard,
            string key,
            Func<object?, bool> condition,
            AbortMode abortMode = AbortMode.None)
            : this("BlackboardCondition", child, getBlackboard, key, condition, abortMode)
        {
        }

        /// <summary>
        /// Creates a blackboard condition decorator with custom name.
        /// </summary>
        public BlackboardCondition(
            string name,
            IBehaviour<TContext> child,
            Func<TContext, IBlackboard> getBlackboard,
            string key,
            Func<object?, bool> condition,
            AbortMode abortMode = AbortMode.None)
            : base(name, child)
        {
            _getBlackboard = getBlackboard ?? throw new ArgumentNullException(nameof(getBlackboard));
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _abortMode = abortMode;
        }

        /// <summary>
        /// IAbortableObserver: true when this node's blackboard condition changed
        /// and it wants the parent to re-evaluate (LowerPriority/Both mode).
        /// </summary>
        public bool IsAbortRequested => _abortRequested;

        /// <summary>
        /// IAbortableObserver: clears the abort request after parent processes it.
        /// </summary>
        public void ClearAbortRequest() => _abortRequested = false;

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            _conditionChanged = false;
            _abortRequested = false;

            // Only create observer if one doesn't already exist
            // (LowerPriority/Both keep observers alive across terminate)
            if (_abortMode != AbortMode.None && _observerHandle == null)
            {
                var blackboard = _getBlackboard(context);
                _observerHandle = blackboard.Observe(_key, (k, oldVal, newVal) =>
                {
                    _conditionChanged = true;

                    if (_abortMode == AbortMode.LowerPriority || _abortMode == AbortMode.Both)
                    {
                        _abortRequested = true;
                    }
                });
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var blackboard = _getBlackboard(context);
            blackboard.TryGet<object>(_key, out var value);
            var conditionMet = _condition(value);

            // Self or Both abort: abort own running child when condition becomes false
            if (_conditionChanged && (_abortMode == AbortMode.Self || _abortMode == AbortMode.Both))
            {
                _conditionChanged = false;
                if (!conditionMet && Child.Status == BehaviourStatus.Running)
                {
                    Child.Reset();
                    return BehaviourStatus.Failed;
                }
            }

            if (!conditionMet)
            {
                return BehaviourStatus.Failed;
            }

            return Child.Tick(context);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            // Only dispose observer on terminate if NOT in LowerPriority/Both mode.
            // LowerPriority observers must stay alive to signal the parent even when
            // this node is not running (e.g., failed condition that may become true later).
            if (_abortMode != AbortMode.LowerPriority && _abortMode != AbortMode.Both)
            {
                _observerHandle?.Dispose();
                _observerHandle = null;
            }
            base.OnTerminate(status);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _observerHandle?.Dispose();
            _observerHandle = null;
            _conditionChanged = false;
            _abortRequested = false;
            base.DoReset(status);
        }
    }

    /// <summary>
    /// Defines when a decorator should abort its child based on condition changes.
    /// </summary>
    public enum AbortMode
    {
        /// <summary>No abort - condition only checked on entry.</summary>
        None,
        /// <summary>Abort own running child when condition becomes false.</summary>
        Self,
        /// <summary>Signal parent to re-evaluate when condition becomes true.</summary>
        LowerPriority,
        /// <summary>Both Self and LowerPriority.</summary>
        Both
    }
}
