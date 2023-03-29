using System;

namespace BehaviourTree.Decorators
{
    public sealed class RepeaterRenew<TContext> : DecoratorBehaviour<TContext>
    {
        public int RepeatCount { get; private set; }
        public int Counter { get; private set; }
        private readonly Func<TContext, int> _getRepeatCount;

        public RepeaterRenew(IBehaviour<TContext> child, Func<TContext, int> getRepeatCount)
            : this("Repeater", child, getRepeatCount)
        {
        }

        public RepeaterRenew(string name, IBehaviour<TContext> child, Func<TContext, int> getRepeatCount)
            : base(name, child)
        {
            if (getRepeatCount == null) throw new ArgumentNullException(nameof(getRepeatCount));
            _getRepeatCount = getRepeatCount;
        }

        protected override BehaviourStatus Update(TContext context)
        {
            if (Status == BehaviourStatus.Ready)
            {
                RepeatCount = _getRepeatCount?.Invoke(context) ?? 0;
                RepeatCount = (RepeatCount <= 0) ? 1 : RepeatCount;
            }

            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Succeeded)
            {
                Counter++;

                if (Counter < RepeatCount)
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
