using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that caches child execution results for a specified interval.
    /// Returns cached result if called within the interval, otherwise executes child.
    /// </summary>
    /// <typeparam name="TContext">Context type that implements IClock for time tracking</typeparam>
    public sealed class RateLimiter<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        private readonly Func<TContext, long>? _getIntervalInMilliseconds;
        private long? _previousTimestamp;
        private BehaviourStatus _previousChildStatus;
        private long _intervalInMilliseconds;

        /// <summary>
        /// Gets the interval in milliseconds between child executions.
        /// </summary>
        public long IntervalInMilliseconds => _intervalInMilliseconds;

        /// <summary>
        /// Creates a new RateLimiter with a dynamic interval.
        /// </summary>
        /// <param name="child">Child behavior to rate limit</param>
        /// <param name="getIntervalInMilliseconds">Function to get interval from context</param>
        public RateLimiter(IBehaviour<TContext> child, Func<TContext, long> getIntervalInMilliseconds)
            : this("RateLimiter", child, getIntervalInMilliseconds)
        {
        }

        /// <summary>
        /// Creates a new RateLimiter with a dynamic interval and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="child">Child behavior to rate limit</param>
        /// <param name="getIntervalInMilliseconds">Function to get interval from context</param>
        /// <exception cref="ArgumentNullException">Thrown when getIntervalInMilliseconds is null</exception>
        public RateLimiter(string name, IBehaviour<TContext> child, Func<TContext, long> getIntervalInMilliseconds)
            : base(name, child)
        {
            _getIntervalInMilliseconds = getIntervalInMilliseconds ?? throw new ArgumentNullException(nameof(getIntervalInMilliseconds));
        }

        /// <summary>
        /// Creates a new RateLimiter with a fixed interval.
        /// </summary>
        /// <param name="child">Child behavior to rate limit</param>
        /// <param name="intervalInMilliseconds">Interval in milliseconds</param>
        public RateLimiter(IBehaviour<TContext> child, int intervalInMilliseconds)
            : this("RateLimiter", child, intervalInMilliseconds)
        {
        }

        /// <summary>
        /// Creates a new RateLimiter with a fixed interval and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="child">Child behavior to rate limit</param>
        /// <param name="intervalInMilliseconds">Interval in milliseconds</param>
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

        /// <summary>
        /// Called on first tick.
        /// </summary>
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