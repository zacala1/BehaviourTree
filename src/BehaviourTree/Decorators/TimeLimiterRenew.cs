using System;

namespace BehaviourTree.Decorators
{
    public sealed class TimeLimiterRenew<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        private long? _initialTimestamp;
        public long TimeLimitInMilliseconds { get; private set; }
        private readonly Func<TContext, int> _getTimeLimitInMilliseconds;

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

        protected override BehaviourStatus Update(TContext context)
        {
            var currentTimeStamp = context.GetTimeStampInMilliseconds();

            if (_initialTimestamp == null)
            {
                _initialTimestamp = currentTimeStamp;
                TimeLimitInMilliseconds = _getTimeLimitInMilliseconds?.Invoke(context) ?? 1000;
            }

            var elapsedMilliseconds = currentTimeStamp - _initialTimestamp;

            if (elapsedMilliseconds >= TimeLimitInMilliseconds)
            {
                return BehaviourStatus.Failed;
            }

            return Child.Tick(context);
        }

        protected override void OnTerminate(BehaviourStatus status)
        {
            _initialTimestamp = null;
        }

        protected override void DoReset(BehaviourStatus status)
        {
            _initialTimestamp = null;
            base.DoReset(status);
        }
    }
}
