namespace BehaviourTree.Composites
{
    /// <summary>
    /// Reactive sequence that re-evaluates from the beginning every tick.
    /// Allows higher priority children to interrupt lower priority ones.
    /// OPTIMIZED: Cached array access for high-frequency tick operations.
    /// </summary>
    public partial class PrioritySequence<TContext> : CompositeBehaviour<TContext>
    {
        /// <summary>
        /// Creates a priority sequence node with default name.
        /// </summary>
        /// <param name="children">Array of child nodes ordered by priority</param>
        public PrioritySequence(IBehaviour<TContext>[] children) : this("PrioritySequence", children)
        {
        }

        /// <summary>
        /// Creates a priority sequence node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Array of child nodes ordered by priority</param>
        public PrioritySequence(string name, IBehaviour<TContext>[] children) : base(name, children)
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

                // Tick the child (reactive behavior: children are re-evaluated in their current state)
                var childStatus = child.Tick(context);

                // If child succeeds, continue to next child in sequence (don't reset it - reactive behavior)
                if (childStatus == BehaviourStatus.Succeeded)
                {
                    // Continue to next child
                    continue;
                }

                // If child fails or is running, reset all later children and return
                // Reset all children after the current one since they're being skipped
                for (var j = i + 1; j < count; j++)
                {
                    // Only reset children that aren't already Ready to avoid unnecessary resets
                    if (children[j].Status != BehaviourStatus.Ready)
                    {
                        children[j].Reset();
                    }
                }

                return childStatus;
            }

            return BehaviourStatus.Succeeded;
        }

        /// <summary>
        /// Override OnTerminate to prevent resetting children when the parent completes.
        /// For reactive nodes, children maintain state between parent ticks and are reset
        /// explicitly during Update() as needed.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            // Don't call base.OnTerminate which would reset all children
            // Children are reset explicitly during Update() for reactive behavior
        }
    }
}