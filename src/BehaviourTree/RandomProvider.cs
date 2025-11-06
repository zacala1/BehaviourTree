using System;

namespace BehaviourTree
{
    /// <summary>
    /// Thread-safe random number provider for behavior tree nodes.
    /// Uses lock synchronization to ensure thread safety when accessed concurrently.
    /// </summary>
    public sealed class RandomProvider : IRandomProvider
    {
        private static readonly Random Random = new Random();
        private static readonly object Lock = new object();

        /// <summary>
        /// Returns a random double between 0.0 and 1.0.
        /// Thread-safe implementation.
        /// </summary>
        public double NextRandomDouble()
        {
            lock (Lock)
            {
                return Random.NextDouble();
            }
        }

        /// <summary>
        /// Returns a non-negative random integer less than the specified maximum.
        /// Thread-safe implementation.
        /// </summary>
        /// <param name="maxValue">Exclusive upper bound</param>
        public int NextRandomInteger(int maxValue)
        {
            lock (Lock)
            {
                return Random.Next(maxValue);
            }
        }

        /// <summary>
        /// Default thread-safe random provider instance.
        /// </summary>
        public static readonly IRandomProvider Default = new RandomProvider();
    }
}