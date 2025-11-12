using BehaviourTree.Demo.GameEngine;
using System;
using System.Linq;
using Xunit;

namespace BehaviourTree.Demo.SystemTests.GameEngine
{
    public class RingBufferTests
    {
        [Fact]
        public void Constructor_InitializesWithCorrectCapacity()
        {
            // Arrange & Act
            var buffer = new RingBuffer<int>(10);

            // Assert
            Assert.Equal(10, buffer.Capacity);
            Assert.Equal(0, buffer.Count);
            Assert.True(buffer.IsEmpty);
            Assert.False(buffer.IsFull);
        }

        [Fact]
        public void Constructor_ZeroOrNegativeCapacity_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new RingBuffer<int>(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RingBuffer<int>(-1));
        }

        [Fact]
        public void Enqueue_AddsItemToBuffer()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);

            // Act
            var result = buffer.Enqueue(42);

            // Assert
            Assert.True(result);
            Assert.Equal(1, buffer.Count);
            Assert.False(buffer.IsEmpty);
        }

        [Fact]
        public void Enqueue_WhenFull_ReturnsFalse()
        {
            // Arrange
            var buffer = new RingBuffer<int>(3);
            buffer.Enqueue(1);
            buffer.Enqueue(2);
            buffer.Enqueue(3);

            // Act
            var result = buffer.Enqueue(4);

            // Assert
            Assert.False(result);
            Assert.Equal(3, buffer.Count);
        }

        [Fact]
        public void EnqueueOverwrite_WhenFull_OverwritesOldest()
        {
            // Arrange
            var buffer = new RingBuffer<int>(3);
            buffer.Enqueue(1);
            buffer.Enqueue(2);
            buffer.Enqueue(3);

            // Act
            buffer.EnqueueOverwrite(4);

            // Assert
            Assert.Equal(3, buffer.Count);
            Assert.True(buffer.IsFull);

            // Verify oldest item (1) was overwritten
            buffer.TryDequeue(out var first);
            Assert.Equal(2, first);
        }

        [Fact]
        public void TryDequeue_RemovesAndReturnsItem()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);
            buffer.Enqueue(42);

            // Act
            var result = buffer.TryDequeue(out var item);

            // Assert
            Assert.True(result);
            Assert.Equal(42, item);
            Assert.Equal(0, buffer.Count);
            Assert.True(buffer.IsEmpty);
        }

        [Fact]
        public void TryDequeue_WhenEmpty_ReturnsFalse()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);

            // Act
            var result = buffer.TryDequeue(out var item);

            // Assert
            Assert.False(result);
            Assert.Equal(default(int), item);
        }

        [Fact]
        public void TryDequeue_FIFOOrder()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);
            buffer.Enqueue(1);
            buffer.Enqueue(2);
            buffer.Enqueue(3);

            // Act & Assert
            buffer.TryDequeue(out var first);
            Assert.Equal(1, first);

            buffer.TryDequeue(out var second);
            Assert.Equal(2, second);

            buffer.TryDequeue(out var third);
            Assert.Equal(3, third);
        }

        [Fact]
        public void TryPeek_ReturnsItemWithoutRemoving()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);
            buffer.Enqueue(42);

            // Act
            var result = buffer.TryPeek(out var item);

            // Assert
            Assert.True(result);
            Assert.Equal(42, item);
            Assert.Equal(1, buffer.Count);
        }

        [Fact]
        public void TryPeek_WhenEmpty_ReturnsFalse()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);

            // Act
            var result = buffer.TryPeek(out var item);

            // Assert
            Assert.False(result);
            Assert.Equal(default(int), item);
        }

        [Fact]
        public void Clear_RemovesAllItems()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);
            buffer.Enqueue(1);
            buffer.Enqueue(2);
            buffer.Enqueue(3);

            // Act
            buffer.Clear();

            // Assert
            Assert.Equal(0, buffer.Count);
            Assert.True(buffer.IsEmpty);
        }

        [Fact]
        public void CircularBehavior_WorksCorrectly()
        {
            // Arrange
            var buffer = new RingBuffer<int>(3);

            // Act - Fill, empty, and refill
            buffer.Enqueue(1);
            buffer.Enqueue(2);
            buffer.Enqueue(3);

            buffer.TryDequeue(out _);
            buffer.TryDequeue(out _);

            buffer.Enqueue(4);
            buffer.Enqueue(5);

            // Assert - Should contain [3, 4, 5]
            buffer.TryDequeue(out var first);
            buffer.TryDequeue(out var second);
            buffer.TryDequeue(out var third);

            Assert.Equal(3, first);
            Assert.Equal(4, second);
            Assert.Equal(5, third);
        }

        [Fact]
        public void GetEnumerator_IteratesInCorrectOrder()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);
            buffer.Enqueue(1);
            buffer.Enqueue(2);
            buffer.Enqueue(3);

            // Act
            var items = buffer.ToList();

            // Assert
            Assert.Equal(new[] { 1, 2, 3 }, items);
        }

        [Fact]
        public void GetEnumerator_WithWrappedBuffer_IteratesCorrectly()
        {
            // Arrange
            var buffer = new RingBuffer<int>(3);
            buffer.Enqueue(1);
            buffer.Enqueue(2);
            buffer.Enqueue(3);
            buffer.TryDequeue(out _);
            buffer.Enqueue(4);

            // Act
            var items = buffer.ToList();

            // Assert
            Assert.Equal(new[] { 2, 3, 4 }, items);
        }

        [Fact]
        public void CopyTo_CopiesAllElements()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);
            buffer.Enqueue(1);
            buffer.Enqueue(2);
            buffer.Enqueue(3);
            var array = new int[5];

            // Act
            buffer.CopyTo(array, 0);

            // Assert
            Assert.Equal(1, array[0]);
            Assert.Equal(2, array[1]);
            Assert.Equal(3, array[2]);
        }

        [Fact]
        public void CopyTo_WithWrappedBuffer_CopiesCorrectly()
        {
            // Arrange
            var buffer = new RingBuffer<int>(3);
            buffer.Enqueue(1);
            buffer.Enqueue(2);
            buffer.Enqueue(3);
            buffer.TryDequeue(out _);
            buffer.Enqueue(4);
            var array = new int[5];

            // Act
            buffer.CopyTo(array, 1);

            // Assert
            Assert.Equal(2, array[1]);
            Assert.Equal(3, array[2]);
            Assert.Equal(4, array[3]);
        }

        [Fact]
        public void IsFull_ReflectsCorrectState()
        {
            // Arrange
            var buffer = new RingBuffer<int>(2);

            // Act & Assert
            Assert.False(buffer.IsFull);

            buffer.Enqueue(1);
            Assert.False(buffer.IsFull);

            buffer.Enqueue(2);
            Assert.True(buffer.IsFull);

            buffer.TryDequeue(out _);
            Assert.False(buffer.IsFull);
        }
    }
}
