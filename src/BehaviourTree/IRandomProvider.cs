namespace BehaviourTree
{
    /// <summary>
    /// Provides random number generation for behaviors.
    /// </summary>
    public interface IRandomProvider
    {
        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        double NextRandomDouble();

        /// <summary>
        /// Returns a random integer between 0 and the specified maximum value.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound.</param>
        /// <returns>A random integer.</returns>
        int NextRandomInteger(int maxValue);
    }
}