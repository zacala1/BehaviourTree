using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// High-performance collection using ArrayPool to reduce allocations.
    /// Ideal for temporary collections that are frequently created and disposed.
    /// IMPORTANT: Must call Dispose() to return the array to the pool.
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    public sealed class PooledCollection<T> : IList<T>, IReadOnlyList<T>, IDisposable
    {
        private const int DefaultCapacity = 4;
        private const int MaxArrayLength = 0X7FFFFFC7;

        private T[] _items;
        private int _size;
        private readonly ArrayPool<T> _pool;
        private bool _disposed;

        public PooledCollection() : this(DefaultCapacity, ArrayPool<T>.Shared)
        {
        }

        public PooledCollection(int capacity) : this(capacity, ArrayPool<T>.Shared)
        {
        }

        public PooledCollection(int capacity, ArrayPool<T> pool)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _items = capacity == 0 ? Array.Empty<T>() : _pool.Rent(capacity);
            _size = 0;
            _disposed = false;
        }

        public int Count => _size;

        public int Capacity => _items.Length;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                ThrowIfDisposed();
                if (index < 0 || index >= _size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _items[index];
            }
            set
            {
                ThrowIfDisposed();
                if (index < 0 || index >= _size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _items[index] = value;
            }
        }

        public void Add(T item)
        {
            ThrowIfDisposed();
            if (_size == _items.Length)
            {
                EnsureCapacity(_size + 1);
            }
            _items[_size++] = item;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            ThrowIfDisposed();
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            foreach (var item in collection)
            {
                Add(item);
            }
        }

        public void Clear()
        {
            ThrowIfDisposed();
            if (_size > 0)
            {
                Array.Clear(_items, 0, _size);
            }
            _size = 0;
        }

        public bool Contains(T item)
        {
            ThrowIfDisposed();
            if (item == null)
            {
                for (var j = 0; j < _size; j++)
                {
                    if (_items[j] == null)
                        return true;
                }
                return false;
            }

            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < _size; i++)
            {
                if (comparer.Equals(_items[i], item))
                    return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ThrowIfDisposed();
            Array.Copy(_items, 0, array, arrayIndex, _size);
        }

        public int IndexOf(T item)
        {
            ThrowIfDisposed();
            return Array.IndexOf(_items, item, 0, _size);
        }

        public void Insert(int index, T item)
        {
            ThrowIfDisposed();
            if (index < 0 || index > _size)
                throw new ArgumentOutOfRangeException(nameof(index));

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

        public bool Remove(T item)
        {
            ThrowIfDisposed();
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
            ThrowIfDisposed();
            if (index < 0 || index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index));

            _size--;
            if (index < _size)
            {
                Array.Copy(_items, index + 1, _items, index, _size - index);
            }
            _items[_size] = default!;
        }

        public void Sort()
        {
            ThrowIfDisposed();
            Sort(0, Count, null);
        }

        public void Sort(IComparer<T> comparer)
        {
            ThrowIfDisposed();
            Sort(0, Count, comparer);
        }

        public void Sort(int index, int count, IComparer<T>? comparer)
        {
            ThrowIfDisposed();
            Array.Sort(_items, index, count, comparer);
        }

        public Span<T> AsSpan()
        {
            ThrowIfDisposed();
            return new Span<T>(_items, 0, _size);
        }

        public Enumerator GetEnumerator()
        {
            ThrowIfDisposed();
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowIfDisposed();
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowIfDisposed();
            return new Enumerator(this);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_items.Length > 0)
            {
                try
                {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                    _pool.Return(_items, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
#else
                    _pool.Return(_items, clearArray: !typeof(T).IsValueType);
#endif
                }
                catch
                {
                    // Ignore exceptions from pool
                }
            }

            _items = Array.Empty<T>();
            _size = 0;
            _disposed = true;
        }

        private void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                int newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;

                // Prevent overflow
                if ((uint)newCapacity > MaxArrayLength)
                    newCapacity = MaxArrayLength;

                if (newCapacity < min)
                    newCapacity = min;

                var newItems = _pool.Rent(newCapacity);
                if (_size > 0)
                {
                    Array.Copy(_items, 0, newItems, 0, _size);
                }

                var toReturn = _items;
                _items = newItems;

                if (toReturn.Length > 0)
                {
                    try
                    {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                        _pool.Return(toReturn, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
#else
                        _pool.Return(toReturn, clearArray: !typeof(T).IsValueType);
#endif
                    }
                    catch
                    {
                        // Ignore exceptions from pool
                    }
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly PooledCollection<T> _list;
            private int _index;
            private T? _current;

            internal Enumerator(PooledCollection<T> list)
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
