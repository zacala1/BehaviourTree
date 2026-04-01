using System;

namespace BehaviourTree.Behaviours
{
    /// <summary>
    /// Leaf node that waits for a specified duration before succeeding.
    /// Returns Running while waiting, then Success after the duration elapses.
    /// Supports both fixed and dynamic (context-based) wait times.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class Wait<TContext> : BaseBehaviour<TContext>
    {
        private static readonly bool ContextImplementsIClock = typeof(IClock).IsAssignableFrom(typeof(TContext));

        private readonly Func<TContext, long>? _getWaitTimeInMilliseconds;
        private long _waitTimeInMilliseconds;
        private long _initialTimestamp = -1;

        /// <summary>Gets the configured wait duration in milliseconds.</summary>
        public long WaitTimeInMilliseconds => _waitTimeInMilliseconds;

        /// <summary>
        /// Creates a wait node with default name.
        /// </summary>
        /// <param name="waitTimeInMilliseconds">Duration to wait in milliseconds (must be non-negative)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when wait time is negative</exception>
        public Wait(int waitTimeInMilliseconds) : this("Wait", waitTimeInMilliseconds)
        {
        }

        /// <summary>
        /// Creates a wait node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="waitTimeInMilliseconds">Duration to wait in milliseconds (must be non-negative)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when wait time is negative</exception>
        public Wait(string name, int waitTimeInMilliseconds) : base(name)
        {
            if (waitTimeInMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(waitTimeInMilliseconds),
                    "Wait time must be non-negative");
            }

            _waitTimeInMilliseconds = waitTimeInMilliseconds;
        }

        /// <summary>
        /// Creates a wait node with dynamic wait time from context.
        /// Wait time is re-evaluated on each initialization.
        /// </summary>
        /// <param name="getWaitTimeInMilliseconds">Function to get wait time from context</param>
        /// <exception cref="ArgumentNullException">Thrown when function is null</exception>
        public Wait(Func<TContext, long> getWaitTimeInMilliseconds) : this("Wait", getWaitTimeInMilliseconds)
        {
        }

        /// <summary>
        /// Creates a wait node with dynamic wait time from context and specified name.
        /// Wait time is re-evaluated on each initialization.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="getWaitTimeInMilliseconds">Function to get wait time from context</param>
        /// <exception cref="ArgumentNullException">Thrown when function is null</exception>
        public Wait(string name, Func<TContext, long> getWaitTimeInMilliseconds) : base(name)
        {
            _getWaitTimeInMilliseconds = getWaitTimeInMilliseconds ?? throw new ArgumentNullException(nameof(getWaitTimeInMilliseconds));
        }

        /// <summary>
        /// Called on first tick to initialize node state.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            if (_getWaitTimeInMilliseconds != null)
            {
                _waitTimeInMilliseconds = _getWaitTimeInMilliseconds.Invoke(context);
            }
        }

        /// <summary>
        /// Core update logic for this node.
        /// </summary>
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

        /// <summary>
        /// Called when node terminates.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            DoReset(status);
        }

        /// <summary>
        /// Called when node is reset.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _initialTimestamp = -1;
        }
    }
}
