using System;
using System.Buffers;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// Helper utilities for using ArrayPool efficiently.
    /// </summary>
    public static class ArrayPoolHelper
    {
        /// <summary>
        /// Rents an array from the shared pool and copies source data into it.
        /// </summary>
        /// <typeparam name="T">Array element type</typeparam>
        /// <param name="source">Source array to copy</param>
        /// <param name="exactSize">If true, ensures array is exactly the source length</param>
        /// <returns>Rented array with copied data</returns>
        public static T[] RentAndCopy<T>(T[] source, bool exactSize = false)
        {
            if (source == null || source.Length == 0)
                return Array.Empty<T>();

            var array = ArrayPool<T>.Shared.Rent(source.Length);
            Array.Copy(source, array, source.Length);

            if (exactSize && array.Length != source.Length)
            {
                var temp = new T[source.Length];
                Array.Copy(array, temp, source.Length);
                ArrayPool<T>.Shared.Return(array);
                return temp;
            }

            return array;
        }

        /// <summary>
        /// Returns a rented array to the shared pool.
        /// </summary>
        /// <typeparam name="T">Array element type</typeparam>
        /// <param name="array">Array to return</param>
        /// <param name="clearArray">Whether to clear the array</param>
        public static void Return<T>(T[] array, bool clearArray = false)
        {
            if (array != null && array.Length > 0)
            {
                ArrayPool<T>.Shared.Return(array, clearArray);
            }
        }

        /// <summary>
        /// Rents an array of specified minimum size.
        /// </summary>
        /// <typeparam name="T">Array element type</typeparam>
        /// <param name="minimumLength">Minimum array length</param>
        /// <returns>Rented array</returns>
        public static T[] Rent<T>(int minimumLength)
        {
            return ArrayPool<T>.Shared.Rent(minimumLength);
        }
    }
}
