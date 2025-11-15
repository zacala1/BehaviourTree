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

                // Tick the child
                var childStatus = child.Tick(context);

                // For reactive behavior, reset non-running children appropriately
                if (childStatus == BehaviourStatus.Succeeded)
                {
                    // Reset succeeded children so they can be re-evaluated on next tick
                    if (child.Status != BehaviourStatus.Ready)
                    {
                        child.Reset();
                    }
                    // Continue to next child in sequence
                }
                else // Failed or Running
                {
                    // Reset all children after the current one since we're not evaluating them
                    for (var j = i + 1; j < count; j++)
                    {
                        // Only reset children that aren't already Ready to avoid unnecessary resets
                        if (children[j].Status != BehaviourStatus.Ready)
                        {
                            children[j].Reset();
                        }
                    }

                    // Don't reset the child that's Running, but reset if Failed
                    if (childStatus == BehaviourStatus.Failed && child.Status != BehaviourStatus.Ready)
                    {
                        // Reset failed children
                        // The reset will happen in OnTerminate
                    }

                    return childStatus;
                }
            }

            return BehaviourStatus.Succeeded;
        }

        /// <summary>
        /// Override DoReset to maintain child state for reactive behavior.
        /// Children are reset explicitly during Update() as needed, not on parent termination.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            // For reactive nodes, don't reset children when parent terminates
            // Children maintain their state and are reset explicitly during Update()
            // This is different from standard composites which reset all children
        }
    }
}