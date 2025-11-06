namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Always returns success when the child completes.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    public sealed class Succeeder<TContext> : DecoratorBehaviour<TContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Succeeder{TContext}"/> class.
        /// </summary>
        /// <param name="child">The child behavior.</param>
        public Succeeder(IBehaviour<TContext> child) : this("Succeeder", child)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Succeeder{TContext}"/> class.
        /// </summary>
        /// <param name="name">The name of the decorator.</param>
        /// <param name="child">The child behavior.</param>
        public Succeeder(string name, IBehaviour<TContext> child) : base(name, child)
        {
        }

        /// <summary>Core update logic for this node.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Succeeded || childStatus == BehaviourStatus.Failed)
            {
                return BehaviourStatus.Succeeded;
            }

            return childStatus;
        }
    }
}