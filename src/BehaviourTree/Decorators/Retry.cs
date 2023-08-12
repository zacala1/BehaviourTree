using System;

namespace BehaviourTree.Decorators
{
    public sealed class Retry<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly Func<TContext, int> _getRetryCount;
        private int _retryCount;
        private int _counter;

        public int RetryCount => _retryCount;
        public int Counter => _counter;

        public Retry(IBehaviour<TContext> child, Func<TContext, int> getRetryCount)
            : this("Retry", child, getRetryCount)
        {
        }

        public Retry(string name, IBehaviour<TContext> child, Func<TContext, int> getRetryCount)
            : base(name, child)
        {
            _getRetryCount = getRetryCount ?? throw new ArgumentNullException(nameof(getRetryCount));
        }

        public Retry(IBehaviour<TContext> child, int retryCount)
            : this("Retry", child, retryCount)
        {
        }

        public Retry(string name, IBehaviour<TContext> child, int retryCount)
            : base(name, child)
        {
            if (retryCount < 1)
            {
                throw new ArgumentException("retryCount must be at least one", nameof(retryCount));
            }

            _retryCount = retryCount;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
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
        protected override void OnInitialize(TContext context)
        {
            if (_getRetryCount != null)
            {
                _retryCount = _getRetryCount.Invoke(context);
            }
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