namespace BehaviourTree.Composites
{
    public sealed class PrioritySequence<TContext> : CompositeBehaviour<TContext>
    {
        public PrioritySequence(IBehaviour<TContext>[] children) : this("PrioritySequence", children)
        {
        }

        public PrioritySequence(string name, IBehaviour<TContext>[] children) : base(name, children)
        {
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            // Priority nodes re-evaluate from the beginning every tick
            // This allows higher priority children to interrupt lower priority ones
            for (var i = 0; i < Children.Length; i++)
            {
                var child = Children[i];

                // Reset children that are not currently running to ensure fresh evaluation
                if (child.Status != BehaviourStatus.Running)
                {
                    child.Reset();
                }

                var childStatus = child.Tick(context);

                if (childStatus != BehaviourStatus.Succeeded)
                {
                    // Reset all children after the current one since we're returning
                    for (var j = i + 1; j < Children.Length; j++)
                    {
                        Children[j].Reset();
                    }

                    return childStatus;
                }
            }

            return BehaviourStatus.Succeeded;
        }
    }
}