namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Always returns failure when the child completes.
    /// Useful for forcing failure in selectors to try next option.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class Failer<TContext> : DecoratorBehaviour<TContext>
    {
        /// <summary>
        /// Creates a failer decorator with default name.
        /// </summary>
        /// <param name="child">Child node to always fail</param>
        public Failer(IBehaviour<TContext> child) : this("Failer", child)
        {
        }

        /// <summary>
        /// Creates a failer decorator with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="child">Child node to always fail</param>
        public Failer(string name, IBehaviour<TContext> child) : base(name, child)
        {
        }

        /// <summary>Core update logic for this node.</summary>
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