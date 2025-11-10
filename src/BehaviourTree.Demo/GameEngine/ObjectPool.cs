using System;
using System.Collections.Generic;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// Generic object pool for reducing allocations and GC pressure.
    /// </summary>
    /// <typeparam name="T">Type of objects to pool</typeparam>
    public sealed class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _factory;
        private readonly Action<T>? _reset;
        private readonly int _maxSize;

        /// <summary>
        /// Creates a new object pool.
        /// </summary>
        /// <param name="factory">Factory function to create new instances</param>
        /// <param name="reset">Optional reset action to clean objects before returning to pool</param>
        /// <param name="initialSize">Initial pool size</param>
        /// <param name="maxSize">Maximum pool size</param>
        public ObjectPool(Func<T> factory, Action<T>? reset = null, int initialSize = 32, int maxSize = 1024)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _reset = reset;
            _maxSize = maxSize;
            _pool = new Stack<T>(initialSize);

            // Pre-populate pool
            for (int i = 0; i < initialSize; i++)
            {
                _pool.Push(_factory());
            }
        }

        /// <summary>
        /// Rents an object from the pool.
        /// </summary>
        /// <returns>An object instance</returns>
        public T Rent()
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }

            return _factory();
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        /// <param name="obj">The object to return</param>
        public void Return(T obj)
        {
            if (obj == null)
                return;

            if (_pool.Count < _maxSize)
            {
                _reset?.Invoke(obj);
                _pool.Push(obj);
            }
        }

        /// <summary>
        /// Gets the current number of objects in the pool.
        /// </summary>
        public int Count => _pool.Count;

        /// <summary>
        /// Clears all objects from the pool.
        /// </summary>
        public void Clear()
        {
            _pool.Clear();
        }
    }
}
