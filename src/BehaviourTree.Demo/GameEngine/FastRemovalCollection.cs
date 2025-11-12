using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// High-performance collection with O(1) removal using swap-and-pop technique.
    /// Order is NOT preserved. Ideal for ECS systems where order doesn't matter.
    /// </summary>
    /// <typeparam name="T">Element type (must be reference type for index tracking)</typeparam>
    public sealed class FastRemovalCollection<T> : IEnumerable<T> where T : class
    {
        private const int DefaultCapacity = 4;
        private T[] _items;
        private int _size;
        private readonly Dictionary<T, int> _indexMap;

        public FastRemovalCollection() : this(DefaultCapacity)
        {
        }

        public FastRemovalCollection(int capacity)
        {
            _items = new T[capacity];
            _size = 0;
            _indexMap = new Dictionary<T, int>(capacity);
        }

        public int Count => _size;

        public int Capacity => _items.Length;
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _items[index];
            }
        }
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

        public bool Remove(T item)
        {
            if (item == null)
                return false;

            if (!_indexMap.TryGetValue(item, out int index))
                return false;

            RemoveAtInternal(index, item);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index));

            var item = _items[index];
            RemoveAtInternal(index, item);
        }

        private void RemoveAtInternal(int index, T item)
        {
            _size--;
            _indexMap.Remove(item);

            if (index != _size)
            {
                T lastItem = _items[_size];
                _items[index] = lastItem;
                _indexMap[lastItem] = index;
            }

            _items[_size] = null!;
        }

        public bool Contains(T item)
        {
            return _indexMap.ContainsKey(item);
        }

        public int IndexOf(T item)
        {
            return _indexMap.TryGetValue(item, out int index) ? index : -1;
        }

        public void Clear()
        {
            if (_size > 0)
            {
                Array.Clear(_items, 0, _size);
                _indexMap.Clear();
            }
            _size = 0;
        }

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

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly FastRemovalCollection<T> _list;
            private int _index;
            private T? _current;

            internal Enumerator(FastRemovalCollection<T> list)
            {
                _list = list;
                _index = 0;
                _current = null;
            }

            public readonly void Dispose()
            {
            }

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
