using System;
using System.Runtime.CompilerServices;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// Helper methods for stack allocation optimization.
    /// </summary>
    public static class StackAllocHelpers
    {
        /// <summary>
        /// Recommended maximum size for stack allocation (256 items of reference types = 2048 bytes on 64-bit).
        /// </summary>
        public const int MaxStackAllocSize = 256;

        /// <summary>
        /// Determines if a collection size is suitable for stack allocation.
        /// </summary>
        /// <param name="count">Number of elements</param>
        /// <returns>True if stack allocation is recommended</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ShouldStackAlloc(int count)
        {
            return count > 0 && count <= MaxStackAllocSize;
        }

        /// <summary>
        /// Determines if a collection size is suitable for stack allocation with custom threshold.
        /// </summary>
        /// <param name="count">Number of elements</param>
        /// <param name="threshold">Custom threshold</param>
        /// <returns>True if stack allocation is recommended</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ShouldStackAlloc(int count, int threshold)
        {
            return count > 0 && count <= threshold;
        }

        /// <summary>
        /// Gets the recommended allocation strategy based on count.
        /// </summary>
        /// <param name="count">Number of elements</param>
        /// <returns>Allocation strategy description</returns>
        public static string GetAllocationStrategy(int count)
        {
            if (count == 0)
                return "Empty";
            if (count <= 32)
                return "StackAlloc";
            if (count <= MaxStackAllocSize)
                return "StackAlloc (Large)";
            if (count <= 1024)
                return "ArrayPool";
            return "HeapAlloc";
        }
    }
}
