using System;

namespace BehaviourTree.Decorators
{
    public sealed class UntilSuccessWithinTimeout<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        private readonly Func<TContext, long>? _getTimeoutInMilliseconds;
        private readonly Action<TContext>? _timeoutAction;
        private long _timeoutInMilliseconds;
        private long? _initialTimestamp;
        public long TimeoutInMilliseconds => _timeoutInMilliseconds;

        public UntilSuccessWithinTimeout(IBehaviour<TContext> child, Func<TContext, long> getTimeoutInMilliseconds, Action<TContext>? timeoutAction = null)
            : this("UntilSuccessWithinTimeout", child, getTimeoutInMilliseconds, timeoutAction)
        {
        }

        public UntilSuccessWithinTimeout(string name, IBehaviour<TContext> child, Func<TContext, long> getTimeoutInMilliseconds, Action<TContext>? timeoutAction = null)
            : base(name, child)
        {
            _getTimeoutInMilliseconds = getTimeoutInMilliseconds ?? throw new ArgumentNullException(nameof(getTimeoutInMilliseconds));
            _timeoutAction = timeoutAction;
        }

        public UntilSuccessWithinTimeout(IBehaviour<TContext> child, long timeoutInMilliseconds = default, Action<TContext>? timeoutAction = null)
            : this("UntilSuccessWithinTimeout", child, timeoutInMilliseconds, timeoutAction)
        {
        }

        public UntilSuccessWithinTimeout(string name, IBehaviour<TContext> child, long timeoutInMilliseconds = default, Action<TContext>? timeoutAction = null)
            : base(name, child)
        {
            _timeoutInMilliseconds = timeoutInMilliseconds;
            _timeoutAction = timeoutAction;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var currentTimeStamp = context.GetTimeStampInMilliseconds();

            if (_initialTimestamp == null)
            {
                _initialTimestamp = currentTimeStamp;
                _timeoutInMilliseconds = _getTimeoutInMilliseconds?.Invoke(context) ?? _timeoutInMilliseconds;
            }

            var elapsedMilliseconds = currentTimeStamp - _initialTimestamp;

            if (elapsedMilliseconds >= _timeoutInMilliseconds)
            {
                _timeoutAction?.Invoke(context);
                return BehaviourStatus.Failed;
            }

            var childStatus = Child.Tick(context);
            return (childStatus == BehaviourStatus.Succeeded) ? BehaviourStatus.Succeeded : BehaviourStatus.Running;
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