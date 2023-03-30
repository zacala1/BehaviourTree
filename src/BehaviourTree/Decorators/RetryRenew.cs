using System;

namespace BehaviourTree.Decorators
{
    public sealed class RetryRenew<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly Func<TContext, int> _getRetryCount;
        private int _retryCount;
        private int _counter;

        public int RetryCount => _retryCount;
        public int Counter => _counter;

        public RetryRenew(IBehaviour<TContext> child, Func<TContext, int> getRetryCount)
            : this("Retry", child, getRetryCount)
        {
        }

        public RetryRenew(string name, IBehaviour<TContext> child, Func<TContext, int> getRetryCount)
            : base(name, child)
        {
            if (getRetryCount == null) throw new ArgumentNullException(nameof(getRetryCount));
            _getRetryCount = getRetryCount;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            if (Status == BehaviourStatus.Ready)
            {
                _retryCount = _getRetryCount?.Invoke(context) ?? 0;
                _retryCount = (_retryCount <= 0) ? 1 : _retryCount;
            }

            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Failed)
            {
                _counter++;

                if (_counter < _retryCount)
                {
                    return BehaviourStatus.Running;
                }
            }

            return childStatus;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            _counter = 0;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _counter = 0;
            base.DoReset(status);
        }
    }
}
