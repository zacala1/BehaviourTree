using System;

namespace BehaviourTree.Decorators
{
    public sealed class RetryRenew<TContext> : DecoratorBehaviour<TContext>
    {
        public int RetryCount { get; private set; }
        public int Counter { get; private set; }
        private readonly Func<TContext, int> _getRetryCount;

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

        protected override BehaviourStatus Update(TContext context)
        {
            if (Status == BehaviourStatus.Ready)
            {
                RetryCount = _getRetryCount?.Invoke(context) ?? 0;
                RetryCount = (RetryCount <= 0) ? 1 : RetryCount;
            }

            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Failed)
            {
                Counter++;

                if (Counter < RetryCount)
                {
                    return BehaviourStatus.Running;
                }
            }

            return childStatus;
        }

        protected override void OnTerminate(BehaviourStatus status)
        {
            Counter = 0;
        }

        protected override void DoReset(BehaviourStatus status)
        {
            Counter = 0;
            base.DoReset(status);
        }
    }
}
