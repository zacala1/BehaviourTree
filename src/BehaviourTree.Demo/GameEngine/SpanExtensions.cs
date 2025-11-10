using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// High-performance extension methods using Span&lt;T&gt; and Memory&lt;T&gt;.
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// Shuffles span elements using Fisher-Yates algorithm with zero allocations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Shuffle<T>(this Span<T> items, IRandomProvider randomProvider)
        {
            int n = items.Length;
            while (n > 1)
            {
                n--;
                int k = randomProvider.NextRandomInteger(n + 1);
                (items[k], items[n]) = (items[n], items[k]);
            }
        }

        /// <summary>
        /// Finds the closest Vector2 position in a span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindClosest(this ReadOnlySpan<Vector2> positions, Vector2 target)
        {
            if (positions.Length == 0)
                return -1;

            int closestIndex = 0;
            float minDistanceSq = Vector2.DistanceSquared(positions[0], target);

            for (int i = 1; i < positions.Length; i++)
            {
                float distanceSq = Vector2.DistanceSquared(positions[i], target);
                if (distanceSq < minDistanceSq)
                {
                    minDistanceSq = distanceSq;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        /// <summary>
        /// Clears a span with the default value using fast memory operations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastClear<T>(this Span<T> span)
        {
            span.Clear();
        }

        /// <summary>
        /// Copies elements with boundary checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeCopyTo<T>(this ReadOnlySpan<T> source, Span<T> destination)
        {
            if (source.Length > destination.Length)
                throw new ArgumentException("Destination span is too small", nameof(destination));

            source.CopyTo(destination);
        }

        /// <summary>
        /// Fills a span with a specific value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastFill<T>(this Span<T> span, T value)
        {
            span.Fill(value);
        }
    }
}
