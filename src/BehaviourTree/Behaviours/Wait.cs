namespace BehaviourTree.Behaviours
{
    public sealed class Wait<TContext> : BaseBehaviour<TContext> where TContext : IClock
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
            var currentTimeStamp = context.GetTimeStampInMilliseconds();

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