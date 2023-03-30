using System;

namespace BehaviourTree.Decorators
{
    public sealed class TimeLimiterRenew<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        private readonly Func<TContext, int> _getTimeLimitInMilliseconds;
        private long? _initialTimestamp;
        private long _timeLimitInMilliseconds;

        public long TimeLimitInMilliseconds => _timeLimitInMilliseconds;

        public TimeLimiterRenew(IBehaviour<TContext> child, Func<TContext, int> getTimeLimitInMilliseconds)
            : this("TimeLimiter", child, getTimeLimitInMilliseconds)
        {
        }

        public TimeLimiterRenew(string name, IBehaviour<TContext> child, Func<TContext, int> getTimeLimitInMilliseconds)
            : base(name, child)
        {
            if (getTimeLimitInMilliseconds == null) throw new ArgumentNullException(nameof(getTimeLimitInMilliseconds));
            _getTimeLimitInMilliseconds = getTimeLimitInMilliseconds;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var currentTimeStamp = context.GetTimeStampInMilliseconds();

            if (_initialTimestamp == null)
            {
                _initialTimestamp = currentTimeStamp;
                _timeLimitInMilliseconds = _getTimeLimitInMilliseconds?.Invoke(context) ?? 1000;
            }

            var elapsedMilliseconds = currentTimeStamp - _initialTimestamp;

            if (elapsedMilliseconds >= _timeLimitInMilliseconds)
            {
                return BehaviourStatus.Failed;
            }

            return Child.Tick(context);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            _initialTimestamp = null;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _initialTimestamp = null;
            base.DoReset(status);
        }
    }
}
