using System;

namespace BehaviourTree.Decorators
{
    public sealed class RepeaterRenew<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly Func<TContext, int> _getRepeatCount;
        private int _repeatCount;
        private int _counter;

        public int RepeatCount => _repeatCount;
        public int Counter => _counter;

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

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            if (Status == BehaviourStatus.Ready)
            {
                _repeatCount = _getRepeatCount?.Invoke(context) ?? 0;
                _repeatCount = (_repeatCount <= 0) ? 1 : _repeatCount;
            }

            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Succeeded)
            {
                _counter++;

                if (_counter < _repeatCount)
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
