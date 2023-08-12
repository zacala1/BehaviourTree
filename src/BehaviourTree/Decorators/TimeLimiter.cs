using System;

namespace BehaviourTree.Decorators
{
    public sealed class TimeLimiter<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        private readonly Func<TContext, long> _getTimeLimitInMilliseconds;
        private long _timeLimitInMilliseconds;
        private long? _initialTimestamp;
        public long TimeLimitInMilliseconds => _timeLimitInMilliseconds;

        public TimeLimiter(IBehaviour<TContext> child, Func<TContext, long> getTimeLimitInMilliseconds)
            : this("TimeLimiter", child, getTimeLimitInMilliseconds)
        {
        }

        public TimeLimiter(string name, IBehaviour<TContext> child, Func<TContext, long> getTimeLimitInMilliseconds)
            : base(name, child)
        {
            _getTimeLimitInMilliseconds = getTimeLimitInMilliseconds ?? throw new ArgumentNullException(nameof(getTimeLimitInMilliseconds));
        }

        public TimeLimiter(IBehaviour<TContext> child, int timeLimitInMilliseconds)
            : this("TimeLimiter", child, timeLimitInMilliseconds)
        {
        }

        public TimeLimiter(string name, IBehaviour<TContext> child, int timeLimitInMilliseconds)
            : base(name, child)
        {
            _timeLimitInMilliseconds = timeLimitInMilliseconds;
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

            if (elapsedMilliseconds >= TimeLimitInMilliseconds)
            {
                return BehaviourStatus.Failed;
            }

            return Child.Tick(context);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            if (_getTimeLimitInMilliseconds != null)
            {
                _timeLimitInMilliseconds = _getTimeLimitInMilliseconds.Invoke(context);
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            _initialTimestamp = null;
            base.OnTerminate(status);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _initialTimestamp = null;
            base.DoReset(status);
        }
    }
}