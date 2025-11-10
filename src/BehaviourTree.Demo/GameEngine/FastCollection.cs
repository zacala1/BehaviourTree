using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// Faster and lighter implementation of System.Collections.ObjectModel.Collection{T}
    /// with value types enumerators to avoid allocation in foreach loops, and various
    /// helper functions.
    ///
    /// Based on Stride Game Engine implementation (MIT License)
    /// https://github.com/stride3d/stride
    /// </summary>
    public class FastCollection<T> : IList<T>, IReadOnlyList<T>
    {
        private const int DefaultCapacity = 4;
        private T[] _items;
        private int _size;

        public FastCollection()
        {
            _items = Array.Empty<T>();
        }

        public FastCollection(IEnumerable<T> collection)
        {
            if (collection is ICollection<T> collectionT)
            {
                var count = collectionT.Count;
                _items = new T[count];
                collectionT.CopyTo(_items, 0);
                _size = count;
            }
            else
            {
                _size = 0;
                _items = new T[DefaultCapacity];
                foreach (var item in collection)
                {
                    Add(item);
                }
            }
        }

        public FastCollection(int capacity)
        {
            _items = new T[capacity];
        }

        public int Capacity
        {
            get => _items.Length;
            set
            {
                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        var destinationArray = new T[value];
                        if (_size > 0)
                        {
                            Array.Copy(_items, 0, destinationArray, 0, _size);
                        }
                        _items = destinationArray;
                    }
                    else
                    {
                        _items = Array.Empty<T>();
                    }
                }
            }
        }

        public int Count => _size;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _items[index];
            }
            set
            {
                if (index < 0 || index >= _size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                SetItem(index, value);
            }
        }

        public void Add(T item)
        {
            InsertItem(_size, item);
        }

        public void Clear()
        {
            ClearItems();
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (var j = 0; j < _size; j++)
                {
                    if (_items[j] == null)
                    {
                        return true;
                    }
                }
                return false;
            }

            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < _size; i++)
            {
                if (comparer.Equals(_items[i], item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_items, 0, array, arrayIndex, _size);
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(_items, item, 0, _size);
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index > _size)
                throw new ArgumentOutOfRangeException(nameof(index));
            InsertItem(index, item);
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index));
            RemoveItem(index);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void AddRange<TE>(TE itemsArgs) where TE : IEnumerable<T>
        {
            foreach (var item in itemsArgs)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets a value-type enumerator to avoid allocations in foreach loops.
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void Sort()
        {
            Sort(0, Count, null);
        }

        public void Sort(IComparer<T> comparer)
        {
            Sort(0, Count, comparer);
        }

        public void Sort(int index, int count, IComparer<T>? comparer)
        {
            Array.Sort(_items, index, count, comparer);
        }

        protected virtual void ClearItems()
        {
            if (_size > 0)
            {
                Array.Clear(_items, 0, _size);
            }
            _size = 0;
        }

        protected virtual void InsertItem(int index, T item)
        {
            if (_size == _items.Length)
            {
                EnsureCapacity(_size + 1);
            }
            if (index < _size)
            {
                Array.Copy(_items, index, _items, index + 1, _size - index);
            }
            _items[index] = item;
            _size++;
        }

        protected virtual void RemoveItem(int index)
        {
            _size--;
            if (index < _size)
            {
                Array.Copy(_items, index + 1, _items, index, _size - index);
            }
            _items[_size] = default!;
        }

        protected virtual void SetItem(int index, T item)
        {
            _items[index] = item;
        }

        public bool IsReadOnly => false;

        public void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                var num = (_items.Length == 0) ? DefaultCapacity : (_items.Length * 2);
                if (num < min)
                {
                    num = min;
                }
                Capacity = num;
            }
        }

        /// <summary>
        /// Value-type enumerator to avoid heap allocations during foreach loops.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly FastCollection<T> _list;
            private int _index;
            private T? _current;

            internal Enumerator(FastCollection<T> list)
            {
                _list = list;
                _index = 0;
                _current = default;
            }

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                var list = _list;
                if (_index < list._size)
                {
                    _current = list._items[_index];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                _index = _list._size + 1;
                _current = default;
                return false;
            }

            public readonly T Current => _current!;

            readonly object IEnumerator.Current => Current!;

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default;
            }
        }
    }
}
