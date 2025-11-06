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
            return TimeSpan.FromTicks(DateTime.UtcNow.Ticks).Milliseconds;
        }
    }
}