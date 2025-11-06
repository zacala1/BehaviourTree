namespace BehaviourTree.Behaviours
{
    public sealed class Wait<TContext> : BaseBehaviour<TContext>
    {
        private readonly long _waitTimeInMilliseconds;
        private long? _initialTimestamp;

        public long WaitTimeInMilliseconds => _waitTimeInMilliseconds;

        public Wait(int waitTimeInMilliseconds) : this("Wait", waitTimeInMilliseconds)
        {
        }

        public Wait(string name, int waitTimeInMilliseconds) : base(name)
        {
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