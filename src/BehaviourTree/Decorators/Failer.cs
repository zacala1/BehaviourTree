namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Always returns failure when the child completes.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    public sealed class Failer<TContext> : DecoratorBehaviour<TContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Failer{TContext}"/> class.
        /// </summary>
        /// <param name="child">The child behavior.</param>
        public Failer(IBehaviour<TContext> child) : this("Failer", child)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Failer{TContext}"/> class.
        /// </summary>
        /// <param name="name">The name of the decorator.</param>
        /// <param name="child">The child behavior.</param>
        public Failer(string name, IBehaviour<TContext> child) : base(name, child)
        {
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Succeeded || childStatus == BehaviourStatus.Failed)
            {
                return BehaviourStatus.Failed;
            }

            return childStatus;
        }
    }
}