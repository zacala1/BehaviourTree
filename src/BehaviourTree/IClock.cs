namespace BehaviourTree
{
    /// <summary>
    /// Provides time-related functionality for behaviors.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current timestamp in milliseconds.
        /// </summary>
        /// <returns>The current timestamp in milliseconds.</returns>
        long GetTimeStampInMilliseconds();
    }
}