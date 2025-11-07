using System;

namespace BehaviourTree.Tests.Utils
{
    /// <summary>
    /// Mock implementation of IClock for testing time-dependent behaviors.
    /// Allows manual control of time progression in tests.
    /// </summary>
    public sealed class MockContext : IClock
    {
        private long _timestamp;

        /// <summary>
        /// General purpose counter for tests.
        /// </summary>
        public int Counter { get; set; }

        /// <summary>
        /// Advances the mock clock by the specified number of milliseconds.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to advance</param>
        public void AddMilliseconds(int milliseconds)
        {
            _timestamp += TimeSpan.FromMilliseconds(milliseconds).Ticks;
        }

        /// <summary>
        /// Gets the current mock timestamp in milliseconds.
        /// </summary>
        /// <returns>Current timestamp in milliseconds</returns>
        public long GetTimeStampInMilliseconds()
        {
            return _timestamp;
        }
    }
}
