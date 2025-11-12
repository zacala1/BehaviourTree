using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BehaviourTree.Composites
{
    /// <summary>
    /// Cache-aligned state for Sequence node to improve cache hit rate.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    internal struct CacheAlignedSequenceState
    {
        [FieldOffset(0)]
        public int CurrentChildIndex;
    }

    /// <summary>
    /// Executes children in order until one fails or all succeed.
    /// OPTIMIZED: Cache-aligned state and aggressive inlining for minimal overhead.
    /// </summary>
    public class Sequence<TContext> : CompositeBehaviour<TContext>
    {
        // CACHE OPTIMIZATION: Align hot field to cache line
        private CacheAlignedSequenceState _state;

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

                // OPTIMIZATION: Early return for common failure/running case
                if (childStatus != BehaviourStatus.Succeeded)
                {
                    return childStatus;
                }

                currentIndex++;
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
            _state.CurrentChildIndex = 0;
            base.DoReset(status);
        }
    }
}