using System;

namespace BehaviourTree
{
    /// <summary>
    /// Extension methods for array operations in behavior trees.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Shuffles array elements using the Fisher-Yates algorithm.
        /// </summary>
        /// <typeparam name="T">Type of array elements</typeparam>
        /// <param name="items">Array to shuffle</param>
        /// <param name="randomProvider">Random number provider for shuffle randomization</param>
        /// <returns>New shuffled array (original array is not modified)</returns>
        public static T[] Shuffle<T>(this T[] items, IRandomProvider randomProvider)
        {
            var n = items.Length;
            var newArray = new T[n];
            Array.Copy(items, newArray, n);

            while (n > 1)
            {
                n--;
                var k = randomProvider.NextRandomInteger(n + 1);
                var value = newArray[k];

                newArray[k] = newArray[n];
                newArray[n] = value;
            }

            return newArray;
        }
    }
}