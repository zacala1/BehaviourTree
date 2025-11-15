using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that retries the child behavior until it succeeds or timeout is reached.
    /// Returns running while child fails, succeeds when child succeeds, and fails on timeout.
    /// </summary>
    /// <typeparam name="TContext">Context type that implements IClock for time tracking</typeparam>
    public sealed partial class UntilSuccessWithinTimeout<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        private readonly Func<TContext, long>? _getTimeoutInMilliseconds;
        private readonly Action<TContext>? _timeoutAction;
        private long _timeoutInMilliseconds;
        private long? _initialTimestamp;

        /// <summary>
        /// Gets the timeout duration in milliseconds.
        /// </summary>
        public long TimeoutInMilliseconds => _timeoutInMilliseconds;

        /// <summary>
        /// Creates a new UntilSuccessWithinTimeout with a dynamic timeout.
        /// </summary>
        /// <param name="child">Child behavior to retry until success</param>
        /// <param name="getTimeoutInMilliseconds">Function to get timeout from context</param>
        /// <param name="timeoutAction">Optional action to execute on timeout</param>
        public UntilSuccessWithinTimeout(IBehaviour<TContext> child, Func<TContext, long> getTimeoutInMilliseconds, Action<TContext>? timeoutAction = null)
            : this("UntilSuccessWithinTimeout", child, getTimeoutInMilliseconds, timeoutAction)
        {
        }

        /// <summary>
        /// Creates a new UntilSuccessWithinTimeout with a dynamic timeout and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="child">Child behavior to retry until success</param>
        /// <param name="getTimeoutInMilliseconds">Function to get timeout from context</param>
        /// <param name="timeoutAction">Optional action to execute on timeout</param>
        /// <exception cref="ArgumentNullException">Thrown when getTimeoutInMilliseconds is null</exception>
        public UntilSuccessWithinTimeout(string name, IBehaviour<TContext> child, Func<TContext, long> getTimeoutInMilliseconds, Action<TContext>? timeoutAction = null)
            : base(name, child)
        {
            _getTimeoutInMilliseconds = getTimeoutInMilliseconds ?? throw new ArgumentNullException(nameof(getTimeoutInMilliseconds));
            _timeoutAction = timeoutAction;
        }

        /// <summary>
        /// Creates a new UntilSuccessWithinTimeout with a fixed timeout.
        /// </summary>
        /// <param name="child">Child behavior to retry until success</param>
        /// <param name="timeoutInMilliseconds">Timeout in milliseconds</param>
        /// <param name="timeoutAction">Optional action to execute on timeout</param>
        public UntilSuccessWithinTimeout(IBehaviour<TContext> child, long timeoutInMilliseconds = default, Action<TContext>? timeoutAction = null)
            : this("UntilSuccessWithinTimeout", child, timeoutInMilliseconds, timeoutAction)
        {
        }

        /// <summary>
        /// Creates a new UntilSuccessWithinTimeout with a fixed timeout and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="child">Child behavior to retry until success</param>
        /// <param name="timeoutInMilliseconds">Timeout in milliseconds</param>
        /// <param name="timeoutAction">Optional action to execute on timeout</param>
        public UntilSuccessWithinTimeout(string name, IBehaviour<TContext> child, long timeoutInMilliseconds = default, Action<TContext>? timeoutAction = null)
            : base(name, child)
        {
            _timeoutInMilliseconds = timeoutInMilliseconds;
            _timeoutAction = timeoutAction;
        }

        /// <summary>
        /// Core update logic for this node.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var currentTimeStamp = context.GetTimeStampInMilliseconds();

            if (_initialTimestamp == null)
            {
                _initialTimestamp = currentTimeStamp;
                _timeoutInMilliseconds = _getTimeoutInMilliseconds?.Invoke(context) ?? _timeoutInMilliseconds;
            }

            var elapsedMilliseconds = currentTimeStamp - _initialTimestamp;

            if (elapsedMilliseconds >= _timeoutInMilliseconds)
            {
                _timeoutAction?.Invoke(context);
                return BehaviourStatus.Failed;
            }

            var childStatus = Child.Tick(context);
            return (childStatus == BehaviourStatus.Succeeded) ? BehaviourStatus.Succeeded : BehaviourStatus.Running;
        }

        /// <summary>
        /// Called when node terminates.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            _initialTimestamp = null;
            base.OnTerminate(status);
        }

        /// <summary>
        /// Called when node is reset.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _initialTimestamp = null;
            base.DoReset(status);
        }
    }
}