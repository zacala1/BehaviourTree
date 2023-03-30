using System;

namespace BehaviourTree.Decorators
{
    public sealed class Retry<TContext> : DecoratorBehaviour<TContext>
    {
        public readonly int RetryCount;
        private int _counter;
        
        public int Counter => _counter;

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

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Failed)
            {
                _counter++;

                if (_counter < RetryCount)
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
