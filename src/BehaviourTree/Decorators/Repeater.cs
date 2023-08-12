using System;

namespace BehaviourTree.Decorators
{
    public sealed class Repeater<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly Func<TContext, int> _getRepeatCount;
        private int _repeatCount;
        private int _counter;

        public int RepeatCount => _repeatCount;
        public int Counter => _counter;

        public Repeater(IBehaviour<TContext> child, Func<TContext, int> getRepeatCount)
            : this("Repeater", child, getRepeatCount)
        {
        }

        public Repeater(string name, IBehaviour<TContext> child, Func<TContext, int> getRepeatCount)
            : base(name, child)
        {
            _getRepeatCount = getRepeatCount ?? throw new ArgumentNullException(nameof(getRepeatCount));
        }

        public Repeater(IBehaviour<TContext> child, int repeatCount)
            : this("Repeater", child, repeatCount)
        {
        }

        public Repeater(string name, IBehaviour<TContext> child, int repeatCount)
            : base(name, child)
        {
            if (repeatCount < 1)
            {
                throw new ArgumentException("repeatCount must be at least one", nameof(repeatCount));
            }

            _repeatCount = repeatCount;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
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
        protected override void OnInitialize(TContext context)
        {
            if (_getRepeatCount != null)
            {
                _repeatCount = _getRepeatCount.Invoke(context);
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