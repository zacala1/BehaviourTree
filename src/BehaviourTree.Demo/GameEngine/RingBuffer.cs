using System;
using System.Collections;
using System.Collections.Generic;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// High-performance circular buffer (ring buffer) with fixed capacity.
    /// Provides O(1) enqueue and dequeue operations with zero allocations.
    /// </summary>
    /// <typeparam name="T">Type of elements in the buffer</typeparam>
    public sealed class RingBuffer<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;
        private int _head;
        private int _tail;
        private int _count;

        /// <summary>
        /// Creates a new ring buffer with specified capacity.
        /// </summary>
        /// <param name="capacity">Maximum number of elements</param>
        public RingBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        /// <summary>
        /// Gets the number of elements in the buffer.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets the capacity of the buffer.
        /// </summary>
        public int Capacity => _buffer.Length;

        /// <summary>
        /// Gets whether the buffer is full.
        /// </summary>
        public bool IsFull => _count == _buffer.Length;

        /// <summary>
        /// Gets whether the buffer is empty.
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        /// Adds an element to the end of the buffer.
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns>True if added successfully, false if buffer is full</returns>
        public bool Enqueue(T item)
        {
            if (IsFull)
                return false;

            _buffer[_tail] = item;
            _tail = (_tail + 1) % _buffer.Length;
            _count++;
            return true;
        }

        /// <summary>
        /// Adds an element to the end of the buffer, overwriting oldest element if full.
        /// </summary>
        /// <param name="item">Item to add</param>
        public void EnqueueOverwrite(T item)
        {
            _buffer[_tail] = item;
            _tail = (_tail + 1) % _buffer.Length;

            if (IsFull)
            {
                _head = (_head + 1) % _buffer.Length;
            }
            else
            {
                _count++;
            }
        }

        /// <summary>
        /// Removes and returns the element at the beginning of the buffer.
        /// </summary>
        /// <param name="item">The dequeued item</param>
        /// <returns>True if an item was dequeued, false if buffer is empty</returns>
        public bool TryDequeue(out T item)
        {
            if (IsEmpty)
            {
                item = default!;
                return false;
            }

            item = _buffer[_head];
            _buffer[_head] = default!;
            _head = (_head + 1) % _buffer.Length;
            _count--;
            return true;
        }

        /// <summary>
        /// Returns the element at the beginning of the buffer without removing it.
        /// </summary>
        /// <param name="item">The peeked item</param>
        /// <returns>True if an item was peeked, false if buffer is empty</returns>
        public bool TryPeek(out T item)
        {
            if (IsEmpty)
            {
                item = default!;
                return false;
            }

            item = _buffer[_head];
            return true;
        }

        /// <summary>
        /// Removes all elements from the buffer.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        /// <summary>
        /// Copies buffer elements to an array.
        /// </summary>
        /// <param name="array">Target array</param>
        /// <param name="arrayIndex">Starting index in target array</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex + _count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (_head < _tail)
            {
                Array.Copy(_buffer, _head, array, arrayIndex, _count);
            }
            else if (_count > 0)
            {
                int firstPartLength = _buffer.Length - _head;
                Array.Copy(_buffer, _head, array, arrayIndex, firstPartLength);
                Array.Copy(_buffer, 0, array, arrayIndex + firstPartLength, _tail);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            int index = _head;
            for (int i = 0; i < _count; i++)
            {
                yield return _buffer[index];
                index = (index + 1) % _buffer.Length;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
