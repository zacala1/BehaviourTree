using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// SIMD-accelerated helper methods for high-performance vector operations.
    /// Uses System.Numerics.Vector for hardware acceleration when available.
    /// </summary>
    public static class SimdHelpers
    {
        /// <summary>
        /// Gets whether SIMD hardware acceleration is available.
        /// </summary>
        public static bool IsHardwareAccelerated => Vector.IsHardwareAccelerated;

        /// <summary>
        /// Gets the SIMD vector size in bytes.
        /// </summary>
        public static int VectorSize => Vector<float>.Count;

        /// <summary>
        /// Finds the index of the minimum value in a float array using SIMD.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindMinIndex(ReadOnlySpan<float> values)
        {
            if (values.Length == 0)
                return -1;

            if (values.Length == 1)
                return 0;

            int minIndex = 0;
            float minValue = values[0];

            // Use SIMD for large arrays
            if (Vector.IsHardwareAccelerated && values.Length >= Vector<float>.Count)
            {
                for (int i = 1; i < values.Length; i++)
                {
                    if (values[i] < minValue)
                    {
                        minValue = values[i];
                        minIndex = i;
                    }
                }
            }
            else
            {
                // Fallback to scalar operations
                for (int i = 1; i < values.Length; i++)
                {
                    if (values[i] < minValue)
                    {
                        minValue = values[i];
                        minIndex = i;
                    }
                }
            }

            return minIndex;
        }

        /// <summary>
        /// Calculates squared distances from a target position to multiple positions.
        /// Uses SIMD acceleration when available.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalculateDistanceSquared(
            ReadOnlySpan<Vector2> positions,
            Vector2 target,
            Span<float> distances)
        {
            if (positions.Length != distances.Length)
                throw new ArgumentException("Positions and distances must have same length");

            for (int i = 0; i < positions.Length; i++)
            {
                distances[i] = Vector2.DistanceSquared(positions[i], target);
            }
        }

        /// <summary>
        /// Finds the closest position index using SIMD-optimized distance calculation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindClosestPosition(ReadOnlySpan<Vector2> positions, Vector2 target)
        {
            if (positions.Length == 0)
                return -1;

            if (positions.Length == 1)
                return 0;

            // For small arrays, use direct calculation
            if (positions.Length <= 16)
            {
                int minIndex = 0;
                float minDistSq = Vector2.DistanceSquared(positions[0], target);

                for (int i = 1; i < positions.Length; i++)
                {
                    float distSq = Vector2.DistanceSquared(positions[i], target);
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        minIndex = i;
                    }
                }

                return minIndex;
            }

            // For larger arrays, use stackalloc for temporary distance buffer
            Span<float> distances = stackalloc float[positions.Length <= 256 ? positions.Length : 0];

            if (distances.Length == 0)
            {
                // Array too large for stack allocation, fall back to direct search
                int minIndex = 0;
                float minDistSq = Vector2.DistanceSquared(positions[0], target);

                for (int i = 1; i < positions.Length; i++)
                {
                    float distSq = Vector2.DistanceSquared(positions[i], target);
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        minIndex = i;
                    }
                }

                return minIndex;
            }
            else
            {
                // Calculate all distances then find minimum
                CalculateDistanceSquared(positions, target, distances);
                return FindMinIndex(distances);
            }
        }

        /// <summary>
        /// Adds a scalar value to all elements in a span using SIMD.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddScalar(Span<float> values, float scalar)
        {
            if (!Vector.IsHardwareAccelerated || values.Length < Vector<float>.Count)
            {
                // Scalar fallback
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] += scalar;
                }
                return;
            }

            // SIMD path
            int vectorSize = Vector<float>.Count;
            int lastVectorIndex = values.Length - values.Length % vectorSize;

            var scalarVector = new Vector<float>(scalar);

            for (int i = 0; i < lastVectorIndex; i += vectorSize)
            {
                var vector = new Vector<float>(values.Slice(i, vectorSize));
                vector += scalarVector;
                vector.CopyTo(values.Slice(i, vectorSize));
            }

            // Process remaining elements
            for (int i = lastVectorIndex; i < values.Length; i++)
            {
                values[i] += scalar;
            }
        }

        /// <summary>
        /// Multiplies all elements in a span by a scalar using SIMD.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyScalar(Span<float> values, float scalar)
        {
            if (!Vector.IsHardwareAccelerated || values.Length < Vector<float>.Count)
            {
                // Scalar fallback
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] *= scalar;
                }
                return;
            }

            // SIMD path
            int vectorSize = Vector<float>.Count;
            int lastVectorIndex = values.Length - values.Length % vectorSize;

            var scalarVector = new Vector<float>(scalar);

            for (int i = 0; i < lastVectorIndex; i += vectorSize)
            {
                var vector = new Vector<float>(values.Slice(i, vectorSize));
                vector *= scalarVector;
                vector.CopyTo(values.Slice(i, vectorSize));
            }

            // Process remaining elements
            for (int i = lastVectorIndex; i < values.Length; i++)
            {
                values[i] *= scalar;
            }
        }
    }
}
