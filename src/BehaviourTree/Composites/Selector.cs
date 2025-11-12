using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BehaviourTree.Composites
{
    /// <summary>
    /// Cache-aligned state for Selector node to improve cache hit rate.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    internal struct CacheAlignedSelectorState
    {
        [FieldOffset(0)]
        public int CurrentChildIndex;
    }

    /// <summary>
    /// Executes children in order until one succeeds or all fail.
    /// OPTIMIZED: Cache-aligned state and aggressive inlining for minimal overhead.
    /// </summary>
    public class Selector<TContext> : CompositeBehaviour<TContext>
    {
        // CACHE OPTIMIZATION: Align hot field to cache line
        private CacheAlignedSelectorState _state;

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
        /// OPTIMIZED: Cache-aligned state, aggressive inlining, and direct array access.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        protected override BehaviourStatus Update(TContext context)
        {
            // OPTIMIZATION: Use direct array access for better performance
            var children = Children;
            var count = children.Length;

            // OPTIMIZATION: Local copy of index for better register allocation
            ref var currentIndex = ref _state.CurrentChildIndex;

            while (currentIndex < count)
            {
                var childStatus = children[currentIndex].Tick(context);

                // OPTIMIZATION: Early return for common success/running case
                if (childStatus != BehaviourStatus.Failed)
                {
                    return childStatus;
                }

                currentIndex++;
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
            _state.CurrentChildIndex = 0;
            base.DoReset(status);
        }
    }
}