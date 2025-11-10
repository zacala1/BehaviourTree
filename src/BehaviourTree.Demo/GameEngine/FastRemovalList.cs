using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// High-performance list optimized for O(1) removal using swap-and-pop technique.
    /// Order is NOT preserved during removal operations.
    /// Ideal for ECS systems where entity order doesn't matter.
    ///
    /// Performance characteristics:
    /// - Add: O(1) amortized
    /// - Remove: O(1) - swaps with last element and removes
    /// - Iteration: O(n) with zero-allocation value-type enumerator
    /// - Lookup: O(1) via internal index tracking
    /// </summary>
    /// <typeparam name="T">Element type (must be reference type for index tracking)</typeparam>
    public sealed class FastRemovalList<T> : IEnumerable<T> where T : class
    {
        private const int DefaultCapacity = 4;
        private T[] _items;
        private int _size;

        // Maps item to its current index in the array for O(1) removal
        private readonly Dictionary<T, int> _indexMap;

        public FastRemovalList() : this(DefaultCapacity)
        {
        }

        public FastRemovalList(int capacity)
        {
            _items = new T[capacity];
            _size = 0;
            _indexMap = new Dictionary<T, int>(capacity);
        }

        /// <summary>
        /// Gets the number of elements in the list.
        /// </summary>
        public int Count => _size;

        /// <summary>
        /// Gets the current capacity of the internal array.
        /// </summary>
        public int Capacity => _items.Length;

        /// <summary>
        /// Gets the element at the specified index.
        /// WARNING: Index may change after Remove operations due to swap-and-pop.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _items[index];
            }
        }

        /// <summary>
        /// Adds an item to the list in O(1) time.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_indexMap.ContainsKey(item))
                throw new ArgumentException("Item already exists in the list", nameof(item));

            if (_size == _items.Length)
            {
                EnsureCapacity(_size + 1);
            }

            _items[_size] = item;
            _indexMap[item] = _size;
            _size++;
        }

        /// <summary>
        /// Removes an item from the list in O(1) time using swap-and-pop.
        /// The last element is moved to the removed element's position.
        /// This means order is NOT preserved.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            if (item == null)
                return false;

            if (!_indexMap.TryGetValue(item, out int index))
                return false;

            RemoveAtInternal(index, item);
            return true;
        }

        /// <summary>
        /// Removes the element at the specified index in O(1) time.
        /// The last element is moved to this position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index));

            var item = _items[index];
            RemoveAtInternal(index, item);
        }

        /// <summary>
        /// Internal O(1) removal implementation using swap-and-pop.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveAtInternal(int index, T item)
        {
            _size--;
            _indexMap.Remove(item);

            // If this is not the last element, swap with the last element
            if (index != _size)
            {
                T lastItem = _items[_size];
                _items[index] = lastItem;
                _indexMap[lastItem] = index;  // Update the moved item's index
            }

            // Clear the last position
            _items[_size] = null!;
        }

        /// <summary>
        /// Checks if the list contains the specified item in O(1) time.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            return _indexMap.ContainsKey(item);
        }

        /// <summary>
        /// Gets the current index of an item in O(1) time.
        /// Returns -1 if the item is not in the list.
        /// WARNING: Index may change after Remove operations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
        {
            return _indexMap.TryGetValue(item, out int index) ? index : -1;
        }

        /// <summary>
        /// Removes all elements from the list.
        /// </summary>
        public void Clear()
        {
            if (_size > 0)
            {
                Array.Clear(_items, 0, _size);
                _indexMap.Clear();
            }
            _size = 0;
        }

        /// <summary>
        /// Ensures the internal array has at least the specified capacity.
        /// </summary>
        private void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                int newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
                if (newCapacity < min)
                {
                    newCapacity = min;
                }

                var newItems = new T[newCapacity];
                if (_size > 0)
                {
                    Array.Copy(_items, 0, newItems, 0, _size);
                }
                _items = newItems;
            }
        }

        /// <summary>
        /// Gets a value-type enumerator for zero-allocation foreach loops.
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Value-type enumerator for zero-allocation iteration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly FastRemovalList<T> _list;
            private int _index;
            private T? _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(FastRemovalList<T> list)
            {
                _list = list;
                _index = 0;
                _current = null;
            }

            public readonly void Dispose()
            {
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_index < _list._size)
                {
                    _current = _list._items[_index];
                    _index++;
                    return true;
                }

                _current = null;
                return false;
            }

            public readonly T Current => _current!;

            readonly object IEnumerator.Current => Current!;

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = null;
            }
        }
    }
}
