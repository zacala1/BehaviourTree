namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Always returns success when the child completes.
    /// Useful for optional actions that shouldn't fail parent sequences.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class Succeeder<TContext> : DecoratorBehaviour<TContext>
    {
        /// <summary>
        /// Creates a succeeder decorator with default name.
        /// </summary>
        /// <param name="child">Child node to always succeed</param>
        public Succeeder(IBehaviour<TContext> child) : this("Succeeder", child)
        {
        }

        /// <summary>
        /// Creates a succeeder decorator with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="child">Child node to always succeed</param>
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