using System;

namespace BehaviourTree.Decorators
{
    public sealed class Retry<TContext> : DecoratorBehaviour<TContext>
    {
        public readonly int RetryCount;
        public int Counter { get; private set; }

        public Retry(IBehaviour<TContext> child, int repeatCount) : this("Retry", child, repeatCount)
        {
        }

        public Retry(string name, IBehaviour<TContext> child, int retryCount) : base(name, child)
        {
            if (retryCount < 1)
            {
                throw new ArgumentException("retryCount must be at least one", nameof(retryCount));
            }

            RetryCount = retryCount;
        }

        protected override BehaviourStatus Update(TContext context)
        {
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
