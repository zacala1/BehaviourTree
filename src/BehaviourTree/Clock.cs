using System;

namespace BehaviourTree
{
    /// <summary>
    /// Default clock implementation that returns current UTC time in milliseconds.
    /// </summary>
    public sealed class Clock : IClock
    {
        /// <summary>
        /// Returns the current UTC timestamp in milliseconds.
        /// </summary>
        public long GetTimeStampInMilliseconds()
        {
            // BUG FIX: Use total milliseconds since epoch, not just milliseconds component (0-999)
            return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}