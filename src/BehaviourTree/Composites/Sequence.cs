using System.Runtime.CompilerServices;

namespace BehaviourTree.Composites
{
    /// <summary>
    /// Executes children in order until one fails or all succeed.
    /// </summary>
    public partial class Sequence<TContext> : CompositeBehaviour<TContext>
    {
        private int _currentChildIndex;

        /// <summary>
        /// Creates a sequence node with default name and variable number of children.
        /// </summary>
        /// <param name="children">Variable number of child nodes to execute</param>
        public Sequence(params IBehaviour<TContext>[] children) : this("Sequence", children)
        {
        }

        /// <summary>
        /// Creates a sequence node with specified name and variable number of children.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Variable number of child nodes to execute</param>
        public Sequence(string name, params IBehaviour<TContext>[] children) : base(name, children)
        {
        }

        /// <summary>
        /// Protected method for subclasses to customize child access (e.g., RandomSequence).
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IBehaviour<TContext> GetChild(int index)
        {
            return Children[index];
        }

        /// <summary>
        /// Executes children sequentially. Returns Success if all succeed, Failed/Running otherwise.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override BehaviourStatus Update(TContext context)
        {
            var children = Children;
            var count = children.Length;

            while (_currentChildIndex < count)
            {
                var childStatus = GetChild(_currentChildIndex).Tick(context);

                if (childStatus != BehaviourStatus.Succeeded)
                {
                    return childStatus;
                }

                _currentChildIndex++;
            }

            return BehaviourStatus.Succeeded;
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