using System;

namespace BehaviourTree.Behaviours
{
    /// <summary>
    /// Behavior that waits for a specified duration, renewed on each initialization.
    /// Returns running while waiting and succeeds when the wait time has elapsed.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    public sealed partial class WaitRenew<TContext> : BaseBehaviour<TContext>
    {
        private static readonly bool ContextImplementsIClock = typeof(IClock).IsAssignableFrom(typeof(TContext));

        private readonly Func<TContext, long> _getWaitTimeInMilliseconds;
        private long _waitTimeInMilliseconds;
        private long _initialTimestamp = -1;

        /// <summary>
        /// Gets the current wait time in milliseconds.
        /// </summary>
        public long WaitTimeInMilliseconds => _waitTimeInMilliseconds;

        /// <summary>
        /// Creates a new WaitRenew behavior with a dynamic wait time.
        /// </summary>
        /// <param name="getWaitTimeInMilliseconds">Function to get wait time from context</param>
        public WaitRenew(Func<TContext, long> getWaitTimeInMilliseconds) : this("Wait", getWaitTimeInMilliseconds)
        {
        }

        /// <summary>
        /// Creates a new WaitRenew behavior with a dynamic wait time and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="getWaitTimeInMilliseconds">Function to get wait time from context</param>
        /// <exception cref="ArgumentNullException">Thrown when getWaitTimeInMilliseconds is null</exception>
        public WaitRenew(string name, Func<TContext, long> getWaitTimeInMilliseconds) : base(name)
        {
            _getWaitTimeInMilliseconds = getWaitTimeInMilliseconds ?? throw new ArgumentNullException(nameof(getWaitTimeInMilliseconds));
        }

        /// <summary>Core update logic for this node.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var currentTimeStamp = GetCurrentTimestamp(context);

            if (_initialTimestamp < 0)
            {
                _initialTimestamp = currentTimeStamp;
            }

            var elapsedMilliseconds = currentTimeStamp - _initialTimestamp;

            if (elapsedMilliseconds >= _waitTimeInMilliseconds)
            {
                return BehaviourStatus.Succeeded;
            }

            return BehaviourStatus.Running;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private static long GetCurrentTimestamp(TContext context)
        {
            if (ContextImplementsIClock)
            {
                return ((IClock)context!).GetTimeStampInMilliseconds();
            }

            return TimeProvider.GetTimestampInMilliseconds();
        }

        /// <summary>Called on first tick to initialize node state.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            _waitTimeInMilliseconds = _getWaitTimeInMilliseconds.Invoke(context);
        }

        /// <summary>Called when node completes execution.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            DoReset(status);
        }

        /// <summary>Resets node state for re-execution.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _initialTimestamp = -1;
        }
    }
}
