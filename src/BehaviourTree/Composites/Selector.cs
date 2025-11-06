namespace BehaviourTree.Composites
{
    /// <summary>
    /// Executes children in order until one succeeds or all fail.
    /// OPTIMIZED: Direct array access for minimal overhead.
    /// </summary>
    public class Selector<TContext> : CompositeBehaviour<TContext>
    {
        private int _currentChildIndex;

        /// <summary>
        /// Creates a selector node with default name.
        /// </summary>
        /// <param name="children">Array of child nodes to execute</param>
        public Selector(IBehaviour<TContext>[] children) : this("Selector", children)
        {
        }

        /// <summary>
        /// Creates a selector node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Array of child nodes to execute</param>
        public Selector(string name, IBehaviour<TContext>[] children) : base(name, children)
        {
        }

        /// <summary>
        /// Protected method for subclasses to customize child access (e.g., RandomSelector).
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected virtual IBehaviour<TContext> GetChild(int index)
        {
            return Children[index];
        }

        /// <summary>
        /// Executes children sequentially. Returns Failed if all fail, Success/Running otherwise.
        /// OPTIMIZED: Uses direct array access for better performance.
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

                if (childStatus != BehaviourStatus.Failed)
                {
                    return childStatus;
                }

                _currentChildIndex++;
            }

            return BehaviourStatus.Failed;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _currentChildIndex = 0;
            base.DoReset(status);
        }
    }
}