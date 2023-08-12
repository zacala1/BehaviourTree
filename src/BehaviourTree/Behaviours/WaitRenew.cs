using System;


namespace BehaviourTree.Behaviours
{
    public sealed class WaitRenew<TContext> : BaseBehaviour<TContext> where TContext : IClock
    {
        private readonly Func<TContext, long> _getWaitTimeInMilliseconds;
        private long _waitTimeInMilliseconds;
        private long? _initialTimestamp;

        public long WaitTimeInMilliseconds => _waitTimeInMilliseconds;
        
        public WaitRenew(Func<TContext, long> getWaitTimeInMilliseconds) : this("Wait", getWaitTimeInMilliseconds)
        {
        }

        public WaitRenew(string name, Func<TContext, long> getWaitTimeInMilliseconds) : base(name)
        {
            _getWaitTimeInMilliseconds = getWaitTimeInMilliseconds ?? throw new ArgumentNullException(nameof(getWaitTimeInMilliseconds));
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
        protected override void OnInitialize(TContext context)
        {
            _waitTimeInMilliseconds = _getWaitTimeInMilliseconds.Invoke(context);
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