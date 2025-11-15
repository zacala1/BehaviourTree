using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that limits child execution time, returning failure if time limit is exceeded.
    /// </summary>
    /// <typeparam name="TContext">Context type that implements IClock for time tracking</typeparam>
    public sealed partial class TimeLimiter<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        private readonly Func<TContext, long>? _getTimeLimitInMilliseconds;
        private long _timeLimitInMilliseconds;
        private long? _initialTimestamp;

        /// <summary>
        /// Gets the time limit in milliseconds.
        /// </summary>
        public long TimeLimitInMilliseconds => _timeLimitInMilliseconds;

        /// <summary>
        /// Creates a new TimeLimiter with a dynamic time limit.
        /// </summary>
        /// <param name="child">Child behavior to time limit</param>
        /// <param name="getTimeLimitInMilliseconds">Function to get time limit from context</param>
        public TimeLimiter(IBehaviour<TContext> child, Func<TContext, long> getTimeLimitInMilliseconds)
            : this("TimeLimiter", child, getTimeLimitInMilliseconds)
        {
        }

        /// <summary>
        /// Creates a new TimeLimiter with a dynamic time limit and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="child">Child behavior to time limit</param>
        /// <param name="getTimeLimitInMilliseconds">Function to get time limit from context</param>
        /// <exception cref="ArgumentNullException">Thrown when getTimeLimitInMilliseconds is null</exception>
        public TimeLimiter(string name, IBehaviour<TContext> child, Func<TContext, long> getTimeLimitInMilliseconds)
            : base(name, child)
        {
            _getTimeLimitInMilliseconds = getTimeLimitInMilliseconds ?? throw new ArgumentNullException(nameof(getTimeLimitInMilliseconds));
        }

        /// <summary>
        /// Creates a new TimeLimiter with a fixed time limit.
        /// </summary>
        /// <param name="child">Child behavior to time limit</param>
        /// <param name="timeLimitInMilliseconds">Time limit in milliseconds</param>
        public TimeLimiter(IBehaviour<TContext> child, int timeLimitInMilliseconds)
            : this("TimeLimiter", child, timeLimitInMilliseconds)
        {
        }

        /// <summary>
        /// Creates a new TimeLimiter with a fixed time limit and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="child">Child behavior to time limit</param>
        /// <param name="timeLimitInMilliseconds">Time limit in milliseconds</param>
        public TimeLimiter(string name, IBehaviour<TContext> child, int timeLimitInMilliseconds)
            : base(name, child)
        {
            _timeLimitInMilliseconds = timeLimitInMilliseconds;
        }

        /// <summary>
        /// Core update logic for this node.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var currentTimeStamp = context.GetTimeStampInMilliseconds();

            if (_initialTimestamp == null)
            {
                _initialTimestamp = currentTimeStamp;
            }

            var elapsedMilliseconds = currentTimeStamp - _initialTimestamp;

            if (elapsedMilliseconds >= TimeLimitInMilliseconds)
            {
                return BehaviourStatus.Failed;
            }

            return Child.Tick(context);
        }

        /// <summary>
        /// Called on first tick.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            if (_getTimeLimitInMilliseconds != null)
            {
                _timeLimitInMilliseconds = _getTimeLimitInMilliseconds.Invoke(context);
            }
        }

        /// <summary>
        /// Called when node terminates.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            _initialTimestamp = null;
            base.OnTerminate(status);
        }

        /// <summary>
        /// Called when node is reset.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _initialTimestamp = null;
            base.DoReset(status);
        }
    }
}