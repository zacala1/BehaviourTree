using System;

namespace BehaviourTree.Decorators
{
    public sealed class RateLimiterRenew<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        private readonly Func<TContext, int> _getIntervalInMilliseconds;
        private long _intervalInMilliseconds;
        private long? _previousTimestamp;
        private BehaviourStatus _previousChildStatus;

        public long IntervalInMilliseconds => _intervalInMilliseconds;
        
        public RateLimiterRenew(IBehaviour<TContext> child, Func<TContext, int> getIntervalInMilliseconds)
            : this("RateLimiter", child, getIntervalInMilliseconds)
        {
        }

        public RateLimiterRenew(string name, IBehaviour<TContext> child, Func<TContext, int> getIntervalInMilliseconds)
            : base(name, child)
        {
            if (getIntervalInMilliseconds == null) throw new ArgumentNullException(nameof(getIntervalInMilliseconds));
            _getIntervalInMilliseconds = getIntervalInMilliseconds;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var currentTimeStamp = context.GetTimeStampInMilliseconds();

            var elapsedMilliseconds = currentTimeStamp - _previousTimestamp;

            if (_previousTimestamp == null || elapsedMilliseconds >= _intervalInMilliseconds)
            {
                _previousChildStatus = Child.Tick(context);

                if (_previousChildStatus != BehaviourStatus.Running)
                {
                    _intervalInMilliseconds = _getIntervalInMilliseconds?.Invoke(context) ?? 0;
                    _previousTimestamp = currentTimeStamp;
                }
            }

            return _previousChildStatus;
        }
    }
}
