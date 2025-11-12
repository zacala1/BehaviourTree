using System;
using System.Threading;

namespace BehaviourTree
{
    /// <summary>
    /// Thread-safe random number provider for behavior tree nodes.
    /// OPTIMIZED: Lock-free implementation using ThreadLocal for zero-contention random generation.
    /// Each thread maintains its own Random instance, eliminating lock contention.
    /// </summary>
    public sealed class RandomProvider : IRandomProvider
    {
        // LOCK-FREE OPTIMIZATION: Per-thread Random instance eliminates all lock contention
        // Each thread gets its own Random instance seeded uniquely
        private static readonly ThreadLocal<Random> ThreadLocalRandom = new ThreadLocal<Random>(
            () => new Random(Guid.NewGuid().GetHashCode()),
            trackAllValues: false  // Don't track for GC optimization
        );

        /// <summary>
        /// Returns a random double between 0.0 and 1.0.
        /// LOCK-FREE: Thread-safe implementation without locks using ThreadLocal.
        /// </summary>
        public double NextRandomDouble()
        {
            // LOCK-FREE: Direct access to thread-local Random, no contention
            return ThreadLocalRandom.Value!.NextDouble();
        }

        /// <summary>
        /// Returns a non-negative random integer less than the specified maximum.
        /// LOCK-FREE: Thread-safe implementation without locks using ThreadLocal.
        /// </summary>
        /// <param name="maxValue">Exclusive upper bound</param>
        public int NextRandomInteger(int maxValue)
        {
            // LOCK-FREE: Direct access to thread-local Random, no contention
            return ThreadLocalRandom.Value!.Next(maxValue);
        }

        /// <summary>
        /// Default lock-free random provider instance.
        /// </summary>
        public static readonly IRandomProvider Default = new RandomProvider();
    }
}