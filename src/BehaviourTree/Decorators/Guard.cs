using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that evaluates a condition every tick before executing the child.
    /// If the condition becomes false while the child is running, the child is
    /// aborted (reset) and the guard returns Failed.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class Guard<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly Func<TContext, bool> _condition;

        /// <summary>
        /// Creates a guard decorator with default name.
        /// </summary>
        /// <param name="child">Child node to execute while condition holds</param>
        /// <param name="condition">Condition evaluated every tick</param>
        /// <exception cref="ArgumentNullException">Thrown when condition is null</exception>
        public Guard(IBehaviour<TContext> child, Func<TContext, bool> condition)
            : this("Guard", child, condition)
        {
        }

        /// <summary>
        /// Creates a guard decorator with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="child">Child node to execute while condition holds</param>
        /// <param name="condition">Condition evaluated every tick</param>
        /// <exception cref="ArgumentNullException">Thrown when condition is null</exception>
        public Guard(string name, IBehaviour<TContext> child, Func<TContext, bool> condition)
            : base(name, child)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <summary>
        /// Evaluates the condition each tick. If false, aborts the child and returns Failed.
        /// If true, ticks the child normally.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            if (!_condition(context))
            {
                // Condition no longer holds - abort running child
                if (Child.Status == BehaviourStatus.Running)
                {
                    Child.Reset();
                }
                return BehaviourStatus.Failed;
            }

            return Child.Tick(context);
        }
    }
}
