using System;

namespace BehaviourTree.Decorators
{
    public sealed class RateLimiter<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        private readonly Func<TContext, long> _getIntervalInMilliseconds;
        private long? _previousTimestamp;
        private BehaviourStatus _previousChildStatus;
        private long _intervalInMilliseconds;

        public long IntervalInMilliseconds => _intervalInMilliseconds;

        public RateLimiter(IBehaviour<TContext> child, Func<TContext, long> getIntervalInMilliseconds)
            : this("RateLimiter", child, getIntervalInMilliseconds)
        {
        }

        public RateLimiter(string name, IBehaviour<TContext> child, Func<TContext, long> getIntervalInMilliseconds)
            : base(name, child)
        {
            _getIntervalInMilliseconds = getIntervalInMilliseconds ?? throw new ArgumentNullException(nameof(getIntervalInMilliseconds));
        }

        public RateLimiter(IBehaviour<TContext> child, int intervalInMilliseconds)
            : this("RateLimiter", child, intervalInMilliseconds)
        {
        }

        public RateLimiter(string name, IBehaviour<TContext> child, int intervalInMilliseconds)
            : base(name, child)
        {
            _intervalInMilliseconds = intervalInMilliseconds;
        }

        /// <summary>
        /// Updates the rate limiter, caching the child's result for the configured interval.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var currentTimeStamp = context.GetTimeStampInMilliseconds();

            // Check if we should execute the child (first run or interval elapsed)
            if (!_previousTimestamp.HasValue ||
                (currentTimeStamp - _previousTimestamp.Value) >= _intervalInMilliseconds)
            {
                _previousChildStatus = Child.Tick(context);

                if (_previousChildStatus != BehaviourStatus.Running)
                {
                    _previousTimestamp = currentTimeStamp;
                }
            }

            return _previousChildStatus;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            if (_getIntervalInMilliseconds != null)
            {
                _intervalInMilliseconds = _getIntervalInMilliseconds.Invoke(context);
            }
        }
    }
}