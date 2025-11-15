using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Repeats the child behavior a specified number of times.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    public sealed partial class Repeater<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly Func<TContext, int>? _getRepeatCount;
        private int _repeatCount;
        private int _counter;

        /// <summary>
        /// Gets the total number of repetitions.
        /// </summary>
        public int RepeatCount => _repeatCount;

        /// <summary>
        /// Gets the current repetition counter.
        /// </summary>
        public int Counter => _counter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repeater{TContext}"/> class with a dynamic repeat count.
        /// </summary>
        /// <param name="child">The child behavior.</param>
        /// <param name="getRepeatCount">Function to get the repeat count from context.</param>
        public Repeater(IBehaviour<TContext> child, Func<TContext, int> getRepeatCount)
            : this("Repeater", child, getRepeatCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Repeater{TContext}"/> class with a dynamic repeat count.
        /// </summary>
        /// <param name="name">The name of the decorator.</param>
        /// <param name="child">The child behavior.</param>
        /// <param name="getRepeatCount">Function to get the repeat count from context.</param>
        public Repeater(string name, IBehaviour<TContext> child, Func<TContext, int> getRepeatCount)
            : base(name, child)
        {
            _getRepeatCount = getRepeatCount ?? throw new ArgumentNullException(nameof(getRepeatCount));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Repeater{TContext}"/> class with a fixed repeat count.
        /// </summary>
        /// <param name="child">The child behavior.</param>
        /// <param name="repeatCount">The number of times to repeat.</param>
        public Repeater(IBehaviour<TContext> child, int repeatCount)
            : this("Repeater", child, repeatCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Repeater{TContext}"/> class with a fixed repeat count.
        /// </summary>
        /// <param name="name">The name of the decorator.</param>
        /// <param name="child">The child behavior.</param>
        /// <param name="repeatCount">The number of times to repeat.</param>
        public Repeater(string name, IBehaviour<TContext> child, int repeatCount)
            : base(name, child)
        {
            if (repeatCount < 1)
            {
                throw new ArgumentException("repeatCount must be at least one", nameof(repeatCount));
            }

            _repeatCount = repeatCount;
        }

        /// <summary>
        /// Core update logic for this node.
        /// </summary>
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

        /// <summary>
        /// Called on first tick.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            if (_getRepeatCount != null)
            {
                _repeatCount = _getRepeatCount.Invoke(context);
            }
        }

        /// <summary>
        /// Called when node terminates.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            _counter = 0;
        }

        /// <summary>
        /// Called when node is reset.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _counter = 0;
            base.DoReset(status);
        }
    }
}