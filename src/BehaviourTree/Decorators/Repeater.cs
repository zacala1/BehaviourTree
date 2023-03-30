using System;

namespace BehaviourTree.Decorators
{
    public sealed class Repeater<TContext> : DecoratorBehaviour<TContext>
    {
        public readonly int RepeatCount;
        private int _counter;
        
        public int Counter => _counter;

        public Repeater(IBehaviour<TContext> child, int repeatCount) : this("Repeater", child, repeatCount)
        {
        }

        public Repeater(string name, IBehaviour<TContext> child, int repeatCount) : base(name, child)
        {
            if (repeatCount < 1)
            {
                throw new ArgumentException("repeatCount must be at least one", nameof(repeatCount));
            }

            RepeatCount = repeatCount;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Succeeded)
            {
                _counter++;

                if (_counter < RepeatCount)
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
