namespace BehaviourTree.Composites
{
    /// <summary>
    /// Reactive sequence that re-evaluates from the beginning every tick.
    /// Allows higher priority children to interrupt lower priority ones.
    /// OPTIMIZED: Cached array access for high-frequency tick operations.
    /// </summary>
    public sealed class PrioritySequence<TContext> : CompositeBehaviour<TContext>
    {
        public PrioritySequence(IBehaviour<TContext>[] children) : this("PrioritySequence", children)
        {
        }

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

                // Reset children that are not currently running to ensure fresh evaluation
                if (child.Status != BehaviourStatus.Running)
                {
                    child.Reset();
                }

                var childStatus = child.Tick(context);

                if (childStatus != BehaviourStatus.Succeeded)
                {
                    // Reset all children after the current one since we're returning
                    for (var j = i + 1; j < count; j++)
                    {
                        children[j].Reset();
                    }

                    return childStatus;
                }
            }

            return BehaviourStatus.Succeeded;
        }
    }
}