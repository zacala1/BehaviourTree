using BehaviourTree.Demo.GameEngine;
using System;
using System.Linq;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.GameEngine
{
    public class RingBufferTests
    {
        [Test]
        public void Constructor_InitializesWithCorrectCapacity()
        {
            // Arrange & Act
            var buffer = new RingBuffer<int>(10);

            // Assert
            Assert.AreEqual(10, buffer.Capacity);
            Assert.AreEqual(0, buffer.Count);
            Assert.IsTrue(buffer.IsEmpty);
            Assert.IsFalse(buffer.IsFull);
        }

        [Test]
        public void Constructor_ZeroOrNegativeCapacity_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new RingBuffer<int>(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RingBuffer<int>(-1));
        }

        [Test]
        public void Enqueue_AddsItemToBuffer()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);

            // Act
            var result = buffer.Enqueue(42);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, buffer.Count);
            Assert.IsFalse(buffer.IsEmpty);
        }

        [Test]
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
            Assert.IsFalse(result);
            Assert.AreEqual(3, buffer.Count);
        }

        [Test]
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
            Assert.AreEqual(3, buffer.Count);
            Assert.IsTrue(buffer.IsFull);

            // Verify oldest item (1) was overwritten
            buffer.TryDequeue(out var first);
            Assert.AreEqual(2, first);
        }

        [Test]
        public void TryDequeue_RemovesAndReturnsItem()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);
            buffer.Enqueue(42);

            // Act
            var result = buffer.TryDequeue(out var item);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(42, item);
            Assert.AreEqual(0, buffer.Count);
            Assert.IsTrue(buffer.IsEmpty);
        }

        [Test]
        public void TryDequeue_WhenEmpty_ReturnsFalse()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);

            // Act
            var result = buffer.TryDequeue(out var item);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(default(int), item);
        }

        [Test]
        public void TryDequeue_FIFOOrder()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);
            buffer.Enqueue(1);
            buffer.Enqueue(2);
            buffer.Enqueue(3);

            // Act & Assert
            buffer.TryDequeue(out var first);
            Assert.AreEqual(1, first);

            buffer.TryDequeue(out var second);
            Assert.AreEqual(2, second);

            buffer.TryDequeue(out var third);
            Assert.AreEqual(3, third);
        }

        [Test]
        public void TryPeek_ReturnsItemWithoutRemoving()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);
            buffer.Enqueue(42);

            // Act
            var result = buffer.TryPeek(out var item);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(42, item);
            Assert.AreEqual(1, buffer.Count);
        }

        [Test]
        public void TryPeek_WhenEmpty_ReturnsFalse()
        {
            // Arrange
            var buffer = new RingBuffer<int>(5);

            // Act
            var result = buffer.TryPeek(out var item);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(default(int), item);
        }

        [Test]
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
            Assert.AreEqual(0, buffer.Count);
            Assert.IsTrue(buffer.IsEmpty);
        }

        [Test]
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

            Assert.AreEqual(3, first);
            Assert.AreEqual(4, second);
            Assert.AreEqual(5, third);
        }

        [Test]
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
            Assert.AreEqual(new[] { 1, 2, 3 }, items);
        }

        [Test]
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
            Assert.AreEqual(new[] { 2, 3, 4 }, items);
        }

        [Test]
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
            Assert.AreEqual(1, array[0]);
            Assert.AreEqual(2, array[1]);
            Assert.AreEqual(3, array[2]);
        }

        [Test]
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
            Assert.AreEqual(2, array[1]);
            Assert.AreEqual(3, array[2]);
            Assert.AreEqual(4, array[3]);
        }

        [Test]
        public void IsFull_ReflectsCorrectState()
        {
            // Arrange
            var buffer = new RingBuffer<int>(2);

            // Act & Assert
            Assert.IsFalse(buffer.IsFull);

            buffer.Enqueue(1);
            Assert.IsFalse(buffer.IsFull);

            buffer.Enqueue(2);
            Assert.IsTrue(buffer.IsFull);

            buffer.TryDequeue(out _);
            Assert.IsFalse(buffer.IsFull);
        }
    }
}
