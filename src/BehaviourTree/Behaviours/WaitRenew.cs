using System;

namespace BehaviourTree.Behaviours
{
    public sealed class WaitRenew<TContext> : BaseBehaviour<TContext> where TContext : IClock
    {
        public long WaitTimeInMilliseconds { get; private set; }
        private readonly Func<TContext, int> _getWaitTimeInMilliseconds;
        private long? _initialTimestamp;

        public WaitRenew(Func<TContext, int> getWaitTimeInMilliseconds)
            : this("Wait", getWaitTimeInMilliseconds)
        {

        }

        public WaitRenew(string name, Func<TContext, int> getWaitTimeInMilliseconds)
            : base(name)
        {
            if (getWaitTimeInMilliseconds == null) throw new ArgumentNullException(nameof(getWaitTimeInMilliseconds));
            _getWaitTimeInMilliseconds = getWaitTimeInMilliseconds;
        }

        protected override BehaviourStatus Update(TContext context)
        {
            var currentTimeStamp = context.GetTimeStampInMilliseconds();

            if (_initialTimestamp == null)
            {
                WaitTimeInMilliseconds = _getWaitTimeInMilliseconds?.Invoke(context) ?? 0;
                _initialTimestamp = currentTimeStamp;
            }

            var elapsedMilliseconds = currentTimeStamp - _initialTimestamp;

            if (elapsedMilliseconds >= WaitTimeInMilliseconds)
            {
                return BehaviourStatus.Succeeded;
            }

            return BehaviourStatus.Running;
        }

        protected override void OnTerminate(BehaviourStatus status)
        {
            DoReset(status);
        }

        protected override void DoReset(BehaviourStatus status)
        {
            _initialTimestamp = null;
        }
    }
}
