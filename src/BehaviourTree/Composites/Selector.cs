using System.Runtime.CompilerServices;

namespace BehaviourTree.Composites
{
    /// <summary>
    /// Executes children in order until one succeeds or all fail.
    /// </summary>
    public partial class Selector<TContext> : CompositeBehaviour<TContext>
    {
        private int _currentChildIndex;

        /// <summary>
        /// Creates a selector node with default name and variable number of children.
        /// </summary>
        /// <param name="children">Variable number of child nodes to execute</param>
        public Selector(params IBehaviour<TContext>[] children) : this("Selector", children)
        {
        }

        /// <summary>
        /// Creates a selector node with specified name and variable number of children.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Variable number of child nodes to execute</param>
        public Selector(string name, params IBehaviour<TContext>[] children) : base(name, children)
        {
        }

        /// <summary>
        /// Protected method for subclasses to customize child access (e.g., RandomSelector).
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IBehaviour<TContext> GetChild(int index)
        {
            return Children[index];
        }

        /// <summary>
        /// Executes children sequentially. Returns Failed if all fail, Success/Running otherwise.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override BehaviourStatus Update(TContext context)
        {
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

        /// <summary>
        /// Resets the current child index when the node is reset.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void DoReset(BehaviourStatus status)
        {
            _currentChildIndex = 0;
            base.DoReset(status);
        }
    }
}
