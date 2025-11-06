using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Repeats the child until it succeeds or a countdown expires.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    public sealed class UntilSuccess<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly Func<TContext, int> _getCountdown;
        private int _countdown;
        private int _counter;

        /// <summary>
        /// Gets the countdown limit.
        /// </summary>
        public int Countdown => _countdown;

        /// <summary>
        /// Gets the current counter value.
        /// </summary>
        public int Counter => _counter;

        public UntilSuccess(IBehaviour<TContext> child, Func<TContext, int> getCountdown)
            : this("UntilSuccess", child, getCountdown)
        {
        }

        public UntilSuccess(string name, IBehaviour<TContext> child, Func<TContext, int> getCountdown)
            : base(name, child)
        {
            _getCountdown = getCountdown ?? throw new ArgumentNullException(nameof(getCountdown));
            _counter = 0;
        }

        public UntilSuccess(IBehaviour<TContext> child, int countdown = default)
            : this("UntilSuccess", child, countdown)
        {
        }

        public UntilSuccess(string name, IBehaviour<TContext> child, int countdown = default)
            : base(name, child)
        {
            _countdown = countdown;
            _counter = 0;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var childStatus = Child.Tick(context);
            if (childStatus == BehaviourStatus.Failed)
            {
                _counter--;
                if (_counter < 0 && Countdown > 0)
                {
                    return BehaviourStatus.Failed;
                }
            }

            return childStatus == BehaviourStatus.Succeeded ? BehaviourStatus.Succeeded : BehaviourStatus.Running;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            if (_getCountdown != null)
            {
                _countdown = _getCountdown.Invoke(context);
            }
            _counter = _countdown;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            _counter = _countdown;
            base.OnTerminate(status);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _counter = _countdown;
            base.DoReset(status);
        }
    }
}