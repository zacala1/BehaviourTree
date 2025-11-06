namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Inverts the child's success and failure status.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    public sealed class Inverter<TContext> : DecoratorBehaviour<TContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Inverter{TContext}"/> class.
        /// </summary>
        /// <param name="child">The child behavior.</param>
        public Inverter(IBehaviour<TContext> child) : this("Inverter", child)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Inverter{TContext}"/> class.
        /// </summary>
        /// <param name="name">The name of the decorator.</param>
        /// <param name="child">The child behavior.</param>
        public Inverter(string name, IBehaviour<TContext> child) : base(name, child)
        {
        }

        /// <summary>Core update logic for this node.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Failed)
            {
                return BehaviourStatus.Succeeded;
            }

            return childStatus == BehaviourStatus.Succeeded ? BehaviourStatus.Failed : childStatus;
        }
    }
}