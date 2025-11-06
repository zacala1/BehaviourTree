namespace BehaviourTree.Composites
{
    /// <summary>
    /// Reactive selector that re-evaluates from the beginning every tick.
    /// Allows higher priority children to interrupt lower priority ones.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public class PrioritySelector<TContext> : CompositeBehaviour<TContext>
    {
        /// <summary>
        /// Creates a priority selector node with default name.
        /// </summary>
        /// <param name="children">Array of child nodes ordered by priority</param>
        public PrioritySelector(IBehaviour<TContext>[] children) : this("PrioritySelector", children)
        {
        }

        /// <summary>
        /// Creates a priority selector node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Array of child nodes ordered by priority</param>
        public PrioritySelector(string name, IBehaviour<TContext>[] children) : base(name, children)
        {
        }

        /// <summary>
        /// Re-evaluates children from the beginning each tick for reactive behavior.
        /// OPTIMIZED: Uses cached array reference and count to minimize overhead.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            // OPTIMIZATION: Cache array reference and length for faster access
            var children = Children;
            var count = children.Length;

            // Priority nodes re-evaluate from the beginning every tick
            // This allows higher priority children to interrupt lower priority ones
            for (var i = 0; i < count; i++)
            {
                var child = children[i];

                // Reset children that are not currently running to ensure fresh evaluation
                if (child.Status != BehaviourStatus.Running)
                {
                    child.Reset();
                }

                var childStatus = child.Tick(context);

                if (childStatus != BehaviourStatus.Failed)
                {
                    // Reset all children after the current one since we're returning
                    for (var j = i + 1; j < count; j++)
                    {
                        children[j].Reset();
                    }

                    return childStatus;
                }
            }

            return BehaviourStatus.Failed;
        }
    }
}