using System;

namespace BehaviourTree
{
    /// <summary>
    /// Global time provider for behavior tree nodes that require timing.
    /// This allows flexibility in time sources (game time, real time, mock time for testing)
    /// without requiring all contexts to implement IClock interface.
    /// </summary>
    public static class TimeProvider
    {
        /// <summary>
        /// Function to get current timestamp in milliseconds.
        /// Default implementation uses UTC DateTime.
        /// Can be overridden for custom time sources (e.g., game engine time, mock time for testing).
        /// </summary>
        public static Func<long> GetTimestampInMilliseconds { get; set; } = DefaultTimeProvider;

        /// <summary>
        /// Default time provider using UTC DateTime
        /// </summary>
        private static long DefaultTimeProvider()
        {
            return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Resets the time provider to default implementation
        /// </summary>
        public static void ResetToDefault()
        {
            GetTimestampInMilliseconds = DefaultTimeProvider;
        }
    }
}
