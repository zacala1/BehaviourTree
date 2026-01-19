namespace BehaviourTree
{
    /// <summary>
    /// Extension methods for array operations in behavior trees.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Shuffles array elements in-place using the Fisher-Yates algorithm.
        /// </summary>
        /// <typeparam name="T">Type of array elements</typeparam>
        /// <param name="items">Array to shuffle (modified in-place)</param>
        /// <param name="randomProvider">Random number provider for shuffle randomization</param>
        public static void ShuffleInPlace<T>(this T[] items, IRandomProvider randomProvider)
        {
            var n = items.Length;

            while (n > 1)
            {
                n--;
                var k = randomProvider.NextRandomInteger(n + 1);
                (items[k], items[n]) = (items[n], items[k]);
            }
        }
    }
}