using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that executes an action after the child succeeds.
    /// Returns the child's status unchanged.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class AfterSuccess<TContext> : DecoratorBehaviour<TContext>
    {
        private BehaviourStatus _childStatus;
        private bool _callbackExecuted;
        private readonly Action<TContext> _action;

        /// <summary>
        /// Creates an after-success decorator with default name.
        /// </summary>
        /// <param name="child">Child node to execute</param>
        /// <param name="action">Action to execute after child succeeds</param>
        public AfterSuccess(IBehaviour<TContext> child, Action<TContext> action) : this("AfterSuccess", child, action)
        {
        }

        /// <summary>
        /// Creates an after-success decorator with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="child">Child node to execute</param>
        /// <param name="action">Action to execute after child succeeds</param>
        /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
        public AfterSuccess(string name, IBehaviour<TContext> child, Action<TContext> action) : base(name, child)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            _action = action;
        }

        /// <summary>Called on first tick to initialize state.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            _childStatus = BehaviourStatus.Ready;
            _callbackExecuted = false;
        }

        /// <summary>Core update logic for this node.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            if (_childStatus == BehaviourStatus.Ready || _childStatus == BehaviourStatus.Running)
            {
                _childStatus = Child.Tick(context);
            }

            if (_childStatus == BehaviourStatus.Succeeded && !_callbackExecuted)
            {
                _callbackExecuted = true;
                try
                {
                    _action.Invoke(context);
                }
                catch
                {
                    // Exceptions in callbacks should not crash the behavior tree
                }
            }

            return _childStatus;
        }

        /// <summary>Called when node completes execution.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            _childStatus = BehaviourStatus.Ready;
        }

        /// <summary>Resets node state for re-execution.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _childStatus = BehaviourStatus.Ready;
            _callbackExecuted = false;
            base.DoReset(status);
        }
    }
}
