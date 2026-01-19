namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Inverts the child's success and failure status.
    /// Success becomes failure, failure becomes success, running unchanged.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class Inverter<TContext> : DecoratorBehaviour<TContext>
    {
        /// <summary>
        /// Creates an inverter decorator with default name.
        /// </summary>
        /// <param name="child">Child node to invert</param>
        public Inverter(IBehaviour<TContext> child) : this("Inverter", child)
        {
        }

        /// <summary>
        /// Creates an inverter decorator with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="child">Child node to invert</param>
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