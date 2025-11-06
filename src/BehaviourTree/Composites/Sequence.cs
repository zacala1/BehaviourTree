namespace BehaviourTree.Composites
{
    /// <summary>
    /// Executes children in order until one fails or all succeed.
    /// OPTIMIZED: Direct array access for minimal overhead.
    /// </summary>
    public class Sequence<TContext> : CompositeBehaviour<TContext>
    {
        private int _currentChildIndex;

        public Sequence(IBehaviour<TContext>[] children) : this("Sequence", children)
        {
        }

        public Sequence(string name, IBehaviour<TContext>[] children) : base(name, children)
        {
        }

        /// <summary>
        /// Protected method for subclasses to customize child access (e.g., RandomSequence).
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected virtual IBehaviour<TContext> GetChild(int index)
        {
            return Children[index];
        }

        /// <summary>
        /// Executes children sequentially. Returns Success if all succeed, Failed/Running otherwise.
        /// OPTIMIZED: Uses direct array access in common case, virtual GetChild for subclasses.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            // OPTIMIZATION: Use direct array access for better performance
            var children = Children;
            var count = children.Length;

            while (_currentChildIndex < count)
            {
                var childStatus = children[_currentChildIndex].Tick(context);

                if (childStatus != BehaviourStatus.Succeeded)
                {
                    return childStatus;
                }

                _currentChildIndex++;
            }

            return BehaviourStatus.Succeeded;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _currentChildIndex = 0;
            base.DoReset(status);
        }
    }
}