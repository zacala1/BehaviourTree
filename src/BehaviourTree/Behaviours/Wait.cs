using System;

namespace BehaviourTree.Behaviours
{
    /// <summary>
    /// Leaf node that waits for a specified duration before succeeding.
    /// Returns Running while waiting, then Success after the duration elapses.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed class Wait<TContext> : BaseBehaviour<TContext>
    {
        private readonly long _waitTimeInMilliseconds;
        private long? _initialTimestamp;

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

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var currentTimeStamp = GetCurrentTimestamp(context);

            if (_initialTimestamp == null)
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
        private long GetCurrentTimestamp(TContext context)
        {
            // Try to get timestamp from context if it implements IClock (backward compatibility)
            if (context is IClock clock)
            {
                return clock.GetTimeStampInMilliseconds();
            }

            // Otherwise use global TimeProvider
            return TimeProvider.GetTimestampInMilliseconds();
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            DoReset(status);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _initialTimestamp = null;
        }
    }
}