using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that executes an action after the child fails.
    /// Returns the child's status unchanged.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class AfterFailed<TContext> : DecoratorBehaviour<TContext>
    {
        private BehaviourStatus childStatus;
        private bool callbackExecuted;
        private readonly Action<TContext> _action;

        /// <summary>
        /// Creates an after-failed decorator with default name.
        /// </summary>
        /// <param name="child">Child node to execute</param>
        /// <param name="action">Action to execute after child fails</param>
        public AfterFailed(IBehaviour<TContext> child, Action<TContext> action) : this("AfterFailed", child, action)
        {
        }

        /// <summary>
        /// Creates an after-failed decorator with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="child">Child node to execute</param>
        /// <param name="action">Action to execute after child fails</param>
        /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
        public AfterFailed(string name, IBehaviour<TContext> child, Action<TContext> action) : base(name, child)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            _action = action;
        }

        /// <summary>Called on first tick to initialize state.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            childStatus = BehaviourStatus.Ready;
            callbackExecuted = false;
        }

        /// <summary>Core update logic for this node.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            // Always tick child until it completes (returns non-Running status)
            if (childStatus == BehaviourStatus.Ready || childStatus == BehaviourStatus.Running)
            {
                childStatus = Child.Tick(context);
            }

            if (childStatus == BehaviourStatus.Failed && !callbackExecuted)
            {
                callbackExecuted = true;
                try
                {
                    _action.Invoke(context);
                }
                catch
                {
                    // Exceptions in callbacks should not crash the behavior tree
                }
            }

            return childStatus;
        }

        /// <summary>Called when node completes execution.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            // Don't reset callbackExecuted here - it should only reset on DoReset
            // This prevents the callback from executing multiple times on consecutive ticks
            childStatus = BehaviourStatus.Ready;
        }

        /// <summary>Resets node state for re-execution.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            childStatus = BehaviourStatus.Ready;
            callbackExecuted = false;
            base.DoReset(status);
        }
    }
}