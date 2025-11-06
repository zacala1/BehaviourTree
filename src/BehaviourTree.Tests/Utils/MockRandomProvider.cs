namespace BehaviourTree.Tests.Utils
{
    /// <summary>
    /// Mock implementation of IRandomProvider for testing random behaviors.
    /// Provides deterministic random values that can be controlled in tests.
    /// </summary>
    public sealed class MockRandomProvider : IRandomProvider
    {
        private double _randomDoubleValue;
        private int _randomIntegerValue;

        /// <summary>
        /// Returns the pre-configured random double value.
        /// </summary>
        /// <returns>Pre-configured double value</returns>
        public double NextRandomDouble()
        {
            return _randomDoubleValue;
        }

        /// <summary>
        /// Returns the pre-configured random integer value.
        /// </summary>
        /// <param name="maxValue">Maximum value (not used in mock)</param>
        /// <returns>Pre-configured integer value</returns>
        public int NextRandomInteger(int maxValue)
        {
            return _randomIntegerValue;
        }

        /// <summary>
        /// Sets the value that will be returned by NextRandomDouble.
        /// </summary>
        /// <param name="value">Value to return</param>
        public void SetNextRandomDouble(double value)
        {
            _randomDoubleValue = value;
        }

        /// <summary>
        /// Sets the value that will be returned by NextRandomInteger.
        /// </summary>
        /// <param name="value">Value to return</param>
        public void SetNextRandomInteger(int value)
        {
            _randomIntegerValue = value;
        }
    }
}